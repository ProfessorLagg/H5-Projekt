export { GameElement, shapes, rangeMapNumber }
// Imports https://html.spec.whatwg.org/multipage/webappapis.html#module-type-allowed
import { TypeChecker } from "./typechecker.mjs"
import * as prng from "./prng.mjs";
import game_css from "./game.css" with { type: "css" };


//#region utils
/**
 * Maps x from the range [srcMin - srcMax] to the range [dstMin - dstMax]
 * @param {Number} x 
 * @param {Number} srcMin 
 * @param {Number} srcMax 
 * @param {Number} dstMin 
 * @param {Number} dstMax 
 * @returns Number
 */
function rangeMapNumber(x, srcMin, srcMax, dstMin, dstMax) {
    return (x - srcMin) / (srcMax - srcMin) * (dstMax - dstMin) + dstMin;
}
/**
 * Sends a non-async get request
 * @param {String} url 
 * @returns The text content of the response
 */
function syncGet(url) {
    let xhttp = new XMLHttpRequest();
    xhttp.open("GET", url, false);
    xhttp.send();
    return xhttp.response;
}
/**
 * Converts 2D board index to 1D board index
 * @param {Number} row 
 * @param {Number} col
 * @param {Boolean} [skip_range_check] Flag to allow out out range input
 * @returns The result 1D index in the range [0-80]
 */
function indexTo1D(row, col, skip_range_check = false) {
    TypeChecker.assertIsInteger(row);
    TypeChecker.assertIsInteger(col);
    if (!skip_range_check && (row < 0 || row > 8)) { throw Error("row must be in the range [0 - 8], but was: " + row) }
    if (!skip_range_check && (col < 0 || col > 8)) { throw Error("col must be in the range [0 - 8], but was: " + col) }

    const r = Math.max(0, Math.min(9, row));
    const c = Math.max(0, Math.min(9, col));
    return r * 9 + c;
}
/**
 * Converts 1D board index to 2D board index
 * @param {Number} index 
 * @param {Boolean} [skip_range_check] Flag to allow out out range input
 * @returns The resulting 2D index row and col
 */
function indexTo2D(index, skip_range_check = false) {
    TypeChecker.assertIsInteger(index);
    if (!skip_range_check && (index < 0 || index > 80)) { throw Error("index must be in the range [0 - 80], but was: " + index) }
    const idx = Math.max(0, Math.min(80, index));
    const col = idx % 9;
    const row = (idx - col) / 9
    return {
        row: row,
        col: col,
    }
}
/**
 * Calculates board group from 2D board index
 * @param {Number} row 
 * @param {Number} col 
 * @returns The resulting board group index
 */
function calcGroup(row, col) {
    TypeChecker.assertIsInteger(row);
    TypeChecker.assertIsInteger(col);
    if (row < 0 || row > 8) { throw Error("row must be in the range [0 - 8], but was: " + row) }
    if (col < 0 || col > 8) { throw Error("col must be in the range [0 - 8], but was: " + col) }
    const r = Math.max(0, Math.min(9, row));
    const c = Math.max(0, Math.min(9, col));
    const gr = Math.floor(r / 3) * 3;
    const gc = Math.floor(c / 3);
    return gr + gc;
}
function urlEncodeSvg(svg) {
    return `data:image/svg+xml;base64,${btoa(svg.outerHTML)}`;
}
/**
 * Checks for any device capable of hovering. In most cases this is a mouse.
 * @returns true if a device exists, otherwise
 */
function user_can_hover() { return matchMedia('(hover: hover)').matches; }
function parseBool(v) {
    if (TypeChecker.isBoolean(v)) { return v; }
    else if (TypeChecker.isInteger(v)) {
        switch (v) {
            case 0: return false;
            case 1: return true;
        }
    } else if (TypeChecker.isBigint(v)) {
        switch (v) {
            case BigInt(false): return false;
            case BigInt(true): return true;
        }
    } else if (TypeChecker.isString(v)) {
        const str = v.toLowerCase();
        switch (str) {
            case "true": return true;
            case "false": return false;
            case "1": return true;
            case "0": return false;
        }
    }

    throw new Error(`could not parse ${v} as a boolean value`)
}
//#endregion

//#region game template
const template_url = import.meta.resolve("./game.html");
function loadTemplate() {
    console.debug("loadTemplate()");
    let txt = syncGet(template_url);
    let elem = new DOMParser().parseFromString(txt, "text/html").head.firstChild;
    elem.id = "tmpl-" + GameElementTagName;
    return elem;
}
//#endregion

//#region Shapes
import shapes from "./shapes.json" with { type: "json" };
const shapeIds = Object.keys(shapes).flatMap(x => Number(x)).filter(x => TypeChecker.isInteger(x) && x >= 0);
const block_url = import.meta.resolve("./block.svg");
const block_data = syncGet(block_url);
const block_data_url = `data:image/svg+xml;base64,${btoa(block_data)}`
const shape_template_url = import.meta.resolve("./shape_template.svg");
const shape_template_string = syncGet(shape_template_url).replaceAll('{block_url}', block_data_url);
const shape_block_string_template = `<rect x="{x}" y="{y}" width="10" height="10" fill="url(#block)" />`
const shapeRenderCache = {};
/**
 * Renders a shape as SVG
 * @param {Number} shapeId id of the shape to render
 * @returns SVG element containing the shape render
 */
function renderShape(shapeId) {
    assertValidShapeId(shapeId);
    console.time(`renderShape(${shapeId})`);
    TypeChecker.assertIsInteger(shapeId);
    if (shapeId < 0 || shapeId >= shapeIds.length) { throw Error("Invalid shapeId: " + shapeId) }

    let svg = shapeRenderCache[shapeId];
    if (svg === undefined) {
        // Update the render cache
        const shape = shapes[shapeId];
        let shapeContent = "";
        for (let i = 0; i < shape.length; i++) {
            const offset = shape[i];
            const x = offset.c * 10;
            const y = offset.r * 10;
            shapeContent += shape_block_string_template
                .replaceAll('{x}', x)
                .replaceAll('{y}', y);
        }
        let svgString = shape_template_string.replace('{content}', shapeContent);
        svg = new DOMParser().parseFromString(svgString, "image/svg+xml").firstChild
        shapeRenderCache[shapeId] = svg;
    }
    console.timeEnd(`renderShape(${shapeId})`);
    return svg;
}
/**
 * @param {Boolean} [allow_empty] enables the empty shape (-1) counting as valid
 */
function validShapeId(shapeId, allow_empty = false) {
    // console.debug(`validShapeId(shapeId:${shapeId}, allow_empty: ${allow_empty})`);
    if (!TypeChecker.isInteger(shapeId)) {
        console.debug("Invalid shapeId: shapeId must be an integer")
        return false;
    }
    if (shapeId < (0 - allow_empty)) {
        console.debug("Invalid shapeId: shapeId must be >= 0")
        return false;
    }
    if (shapeId >= shapes.length) {
        console.debug("Invalid shapeId: shapeId must be < " + shapes.length)
        return false;
    }
    return true;
}
/** 
 * Throws if invalid shapeId
 * @param {Boolean} [allow_empty] enables the empty shape (-1) counting as valid
 */
function assertValidShapeId(shapeId, allow_empty = false) {
    // console.debug(`assertValidShapeId(shapeId:${shapeId}, allow_empty: ${allow_empty})`);
    if (!TypeChecker.isInteger(shapeId)) { throw new Error("Invalid shapeId: shapeId must be an integer") }
    if (shapeId < (0 - allow_empty)) { throw new Error("Invalid shapeId: shapeId must be >= 0") }
    if (shapeId >= shapes.length) { throw new Error("Invalid shapeId: shapeId must be < " + shapes.length) }
    if (!validShapeId(shapeId, allow_empty)) { throw new Error("Invalid shapeId: " + shapeId); }
}
//#endregion



// -- GameElement --
const GameElementTagName = 'game-wrap';
const saveFileKey = 'savefile';
class GameElement extends HTMLElement {
    rand = new prng.sfc32(new Uint32Array(4));
    start() {
        this.uiOverlay.classList.add("hidden");
        this.gameplayWrap.setAttribute("over", "false");
        this.gameplayWrap.classList.add("running");
        this.init();
    }
    restart() {
        console.debug(this.localName, "restart()");
        this.rand = new prng.sfc32(prng.sfc32.generateSeed());
        this.refill_piece_buffer();
        this.start();
        this.save();
    }
    get gameplayWrap() { return this.shadowRoot.getElementById('gamplay-wrap') }
    get uiOverlay() { return this.shadowRoot.getElementById('ui-overlay'); }
    get startButton() { return this.shadowRoot.getElementById('start-button') }
    get resumeButton() { return this.shadowRoot.getElementById('resume-button') }
    startButton_click(e) {
        console.debug("startButton_click");
        this.restart();
    }
    reloadButton_click(e) {
        if (this.can_load()) { this.load(); }
    }
    /**
     * Checks if this game is over
     * @param {Uint8Array} [boardState] Optional pre-generated boardState (output from this.getBoardStateArray())
     * @returns true if there is no placeable, non-empty, piece in the piece-buffer, otherwise false
     */
    checkGameover(boardState = null) {
        const shapeId_1 = parseInt(this.piece1.getAttribute("shapeId"));
        const shapeId_2 = parseInt(this.piece2.getAttribute("shapeId"));
        const shapeId_3 = parseInt(this.piece3.getAttribute("shapeId"));

        // Check if the pieces are empty / spent
        const isEmpty_1 = shapeId_1 === -1;
        const isEmpty_2 = shapeId_2 === -1;
        const isEmpty_3 = shapeId_3 === -1;
        if (isEmpty_1 && isEmpty_2 && isEmpty_3) {
            // the game is not over since the piece-buffer is empty and should therefore just be filled
            console.log("Game not over due to empty piece-buffer");
            return false;
        }

        // check if the un-spent pieces can be placed anywhere
        const state = TypeChecker.isNullOrUndefined(boardState) ? this.getBoardStateArray() : boardState;
        if (!isEmpty_1 && this.canPlaceShapeAnywhere(shapeId_1, state)) { return false; }
        if (!isEmpty_2 && this.canPlaceShapeAnywhere(shapeId_2, state)) { return false; }
        if (!isEmpty_3 && this.canPlaceShapeAnywhere(shapeId_3, state)) { return false; }
        return true;
    }
    /**
     * Attempts to place a piece from the piecebuffer at the specified 1D board-cell index (idx attribute on cell element)
     * @param {string} pieceId css id of the piece to place from the piece-buffer
     * @param {Number} index 1D board-cell index (idx attribute on cell element)
     * @returns true if the piece was placed, otherwise false
     */
    tryPlacePiece(pieceId, index) {
        console.debug(this.localName, "tryPlacePiece()")
        const piece = this.shadowRoot.getElementById(pieceId);
        let idx2D = -1;
        try {
            idx2D = indexTo2D(index);
        } catch {
            console.log("tryPlacePiece():", "invalid board-cell index: ", index);
            return false;
        }

        if (TypeChecker.isNullOrUndefined(piece)) { throw Error("Invalid piece id: " + pieceId); }

        const shapeId = parseInt(piece.getAttribute("shapeId"));

        const boardState = this.getBoardStateArray();
        const canPlace = this.canPlaceShape(shapeId, index);
        if (!canPlace) {
            console.log("tryPlacePiece():", "Could not place piece", pieceId, "at board-cell index", index)
            return false;
        }

        let intersectingCells = this.getIntersectingCells(shapeId, index)
        // TODO write move log
        while (intersectingCells.length > 0) {
            const cell = intersectingCells.pop();
            cell.classList.remove('highlight');
            cell.setAttribute("state", 1);
            this.score += 1;
        }
        piece.setAttribute("shapeId", -1);
        piece.style.backgroundImage = '';
        this.score += this.clear();
        this.refill_piece_buffer_if_empty();
        if (this.checkGameover()) {
            console.log("GAMEOVER!")
            this.endGame();
        } else {
            this.save();
        }
        return true;
    }
    /**Marks this game as over*/
    endGame() {
        this.gameplayWrap.setAttribute("over", "true")
        this.uiOverlay.classList.remove("hidden");
        this.uiOverlay.classList.add("gameover");
        localStorage.removeItem("savefile");
        this.resumeButton.setAttribute("canLoad", this.can_load());
        // TODO save game high-scores
    }
    get over() { return parseBool(this.gameplayWrap.getAttribute("over")); }
    set over(v) { this.gameplayWrap.setAttribute("over", parseBool(v)) }
    /**Saves this game to localstorage*/
    save() {
        if (this.over) { return; }
        const savefile = {
            shapes_version: shapes.version,
            rand: {
                seed: this.rand.seed,
                count: this.rand.count,
                state: this.rand.state,
            },
            score: this.score,
            board: Array.from(game.getBoardStateArray()),
            pieces: [
                this.piece1.getAttribute("shapeId"),
                this.piece2.getAttribute("shapeId"),
                this.piece3.getAttribute("shapeId")
            ],

        }
        localStorage.setItem(saveFileKey, JSON.stringify(savefile));
    }
    /**
     * Loads the current save.
     * Throws if no savefile or invalid savefile
     */
    load() {
        const savefileJSON = localStorage.getItem(saveFileKey);
        if (TypeChecker.isNullOrUndefined(savefileJSON)) { throw Error("No save file found") }
        const savefile = JSON.parse(savefileJSON);
        if (savefile.shapes_version != shapes.version) { throw Error("Savefile uses a different shapes version than is currently loaded") }
        this.start();

        this.rand.seed = savefile.rand.seed;
        this.rand.count = savefile.rand.count;
        this.rand.state = savefile.rand.state;

        this.score = savefile.score;

        for (let i = 0; i < savefile.board.length; i++) {
            const cell = this.getCellByIndex(i);
            cell.setAttribute("state", savefile.board[i]);
        }

        for (let i = 0; i < savefile.pieces.length; i++) {
            const shapeId = parseInt(savefile.pieces[i]);
            const pieceId = `piece-${i + 1}`
            this.setPieceShape(pieceId, shapeId);
        }
    }
    /**Checks for valid savefile to load*/
    can_load() {
        const savefileJSON = localStorage.getItem(saveFileKey);
        if (TypeChecker.isNullOrUndefined(savefileJSON)) { return false; }
        const savefile = JSON.parse(savefileJSON);
        if (savefile.shapes_version != shapes.version) { return false; }
        return true;
    }

    /**
     * Checks if a shape, specified by it's id, can be placed at a 1D board-cell index (idx attribute on cell element)
     * @param {Number} shapeId Id of the shape to check
     * @param {Number} index 1D board-cell index (idx attribute on cell element)
     * @param {Uint8Array} [boardState] Optional pre-generated boardState (output from this.getBoardStateArray())
     */
    canPlaceShape(shapeId, index, boardState = null) {
        assertValidShapeId(shapeId);
        const shape = shapes[shapeId];
        const idx2D = indexTo2D(index);
        const state = TypeChecker.isNullOrUndefined(boardState) ? this.getBoardStateArray() : boardState;
        for (let i = 0; i < shape.length; i++) {
            const row = shape[i].r + idx2D.row;
            const col = shape[i].c + idx2D.col;
            if (row < 0 || row > 8 || col < 0 || col > 8) { return false; }
            const idx = indexTo1D(row, col);
            if (state[idx] === 1) { return false; }
        }
        return true;
    }
    /**
     * Checks if a shape, specified by it's id, can be placed on the current board
     * @param {Number} shapeId Id of the shape to check placement for
     * @param {Uint8Array} [boardState] Optional pre-generated boardState (output from this.getBoardStateArray())
     * @returns true if the shape can be placed, otherwise false
     */
    canPlaceShapeAnywhere(shapeId, boardState = null) {
        assertValidShapeId(shapeId);
        const state = TypeChecker.isNullOrUndefined(boardState) ? this.getBoardStateArray() : boardState;
        for (let idx1D = 0; idx1D < state.length; idx1D++) {
            if (this.canPlaceShape(shapeId, idx1D, state)) {
                return true;
            }
        }
        return false;
    }
    //#region score
    set score(new_score) {
        TypeChecker.assertIsInteger(new_score);
        const old_score = this.score;
        console.debug(`updated score: ${old_score} -> ${new_score}`)
        this.scoreElem.setAttribute("value", new_score);
        this.scoreElem.textContent = new_score.toString();
    }
    get score() {
        return parseInt(this.scoreElem.getAttribute("value"));
    }
    get scoreElem() {
        return this.shadowRoot.getElementById('game-score');
    }
    //#endregion

    //#region Board
    get board() { return this.shadowRoot.getElementById('game-board') }
    get groups() {
        return [
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[grp="0"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[grp="1"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[grp="2"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[grp="3"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[grp="4"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[grp="5"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[grp="6"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[grp="7"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[grp="8"]')),
        ]
    }
    get rows() {
        return [
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[row="0"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[row="1"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[row="2"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[row="3"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[row="4"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[row="5"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[row="6"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[row="7"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[row="8"]')),
        ]
    }
    get columns() {
        return [
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[col="0"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[col="1"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[col="2"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[col="3"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[col="4"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[col="5"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[col="6"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[col="7"]')),
            Array.from(this.shadowRoot.querySelectorAll('.board-cell[col="8"]')),
        ]
    }
    /**
     * Gets the curent board state as a Uint8Array
     * @returns The curent board state as a Uint8Array
     */
    getBoardStateArray() {
        let arr = new Uint8Array(81);
        const cells = this.getCells();
        for (let i = 0; i < cells.length; i++) {
            const idx = parseInt(cells[i].getAttribute("idx"));
            const state = parseInt(cells[i].getAttribute("state"));

            if (idx < 0 || idx > 80) { throw Error("Invalid cell index: " + idx) }
            if (!(state === 1 || state === 0)) { throw Error("Invalid cell state: " + state) }
            arr[idx] = state;
        }
        return arr;
    }
    /** returns all clearable rows, columns & groups*/
    getClearableSections() {
        let clearableSections = [];
        let section = undefined;
        // GROUPS
        const grps = this.groups;
        for (let i = 0; i < grps.length; i++) {
            section = grps[i];
            let filled = true;
            for (let j = 0; j < section.length; j++) {
                const state = parseInt(section[j].getAttribute("state"));
                if (state !== 1) {
                    filled = false;
                    break;
                }
            }
            if (filled) { clearableSections.push(section); }
        }
        section = undefined;

        // ROWS
        const rows = this.rows;
        for (let i = 0; i < rows.length; i++) {
            section = rows[i];
            let filled = true;
            for (let j = 0; j < section.length; j++) {
                const state = parseInt(section[j].getAttribute("state"));
                if (state !== 1) {
                    filled = false;
                    break;
                }
            }
            if (filled) { clearableSections.push(section); }
        }
        section = undefined;

        // COLUMNS
        const cols = this.columns;
        for (let i = 0; i < cols.length; i++) {
            section = cols[i];
            let filled = true;
            for (let j = 0; j < section.length; j++) {
                const state = parseInt(section[j].getAttribute("state"));
                if (state !== 1) {
                    filled = false;
                    break;
                }
            }
            if (filled) { clearableSections.push(section); }
        }
        section = undefined;
        return clearableSections;
    }
    /** clears all clearable cells. returns the delta score*/
    clear() {
        const clearableSections = this.getClearableSections();
        let section = undefined;
        let result = 0;
        for (let i = 0; i < clearableSections.length; i++) {
            section = clearableSections[i];
            for (let j = 0; j < section.length; j++) {
                result += 1;
                section[j].setAttribute("state", 0);
            }
        }
        return result;
    }

    //#region cells
    /**
     * 
     * @returns All the cells on the game-board
     */
    getCells() {
        return Array.from(this.shadowRoot.querySelectorAll(".board-cell"));
    }
    /**
     * 
     * @param {Number} index 
     * @returns The cell at the specified 1D index
     */
    getCellByIndex(index) {
        TypeChecker.assertIsInteger(index);
        if (index < 0 || index > 80) { throw Error("index must be in the range [0 - 80], but was: " + index) }
        return this.shadowRoot.querySelector(`.board-cell[idx="${index}"`);
    }
    /**
     * 
     * @param {Number} row 
     * @param {Number} col 
     * @returns The cell at the specified row, coloumn 2D index
     */
    getCellByRowCol(row, col) {
        TypeChecker.assertIsInteger(row);
        TypeChecker.assertIsInteger(col);
        if (row < 0 || row > 8) { throw Error("row must be in the range [0 - 8], but was: " + row) }
        if (col < 0 || col > 8) { throw Error("col must be in the range [0 - 8], but was: " + col) }
        return this.shadowRoot.querySelector(`.board-cell[row="${row}"][col="${col}"]`)
    }
    /**
     * 
     * @param {Number} row 
     * @returns All the cells in the specified row
     */
    getRow(row) {
        TypeChecker.assertIsInteger(row);
        if (row < 0 || row > 8) { throw Error("row must be in the range [0 - 8], but was: " + row) }
        return Array.from(this.shadowRoot.querySelectorAll(`.board-cell[row="${row}"]`));
    }
    /**
     * 
     * @param {Number} col 
     * @returns All the cells in the specified column
     */
    getColumn(col) {
        TypeChecker.assertIsInteger(col);
        if (col < 0 || col > 8) { throw Error("col must be in the range [0 - 8], but was: " + col) }
        return Array.from(this.shadowRoot.querySelectorAll(`.board-cell[col="${col}"]`));
    }
    /**
     * 
     * @param {Number} grp 
     * @returns All the cells in the specified group
     */
    getGroup(grp) {
        TypeChecker.assertIsInteger(grp);
        if (grp < 0 || grp > 8) { throw Error("grp must be in the range [0 - 8], but was: " + grp) }
        return Array.from(this.shadowRoot.querySelectorAll(`.board-cell[grp="${grp}"]`));
    }
    /**
     * Throws an error if elem is not a board cell in this game.
     * @param {HTMLElement} elem 
     */
    assertIsCell(elem) {
        const errorMsg = `${elem} is not a valid board cell`;
        if (TypeChecker.isNullOrUndefined(elem)) { throw errorMsg; }

        let idx = elem.getAttribute("idx");
        let row = elem.getAttribute("row");
        let col = elem.getAttribute("col");
        let grp = elem.getAttribute("grp");
        let state = elem.getAttribute("state");

        if (TypeChecker.isNullOrUndefined(idx)) { throw Error(errorMsg) }
        if (TypeChecker.isNullOrUndefined(row)) { throw Error(errorMsg) }
        if (TypeChecker.isNullOrUndefined(col)) { throw Error(errorMsg) }
        if (TypeChecker.isNullOrUndefined(grp)) { throw Error(errorMsg) }
        if (TypeChecker.isNullOrUndefined(state)) { throw Error(errorMsg) }

        idx = parseInt(idx);
        row = parseInt(row);
        col = parseInt(col);
        grp = parseInt(grp);
        state = parseInt(state);

        if (!TypeChecker.isInteger(idx)) { throw Error(errorMsg) }
        if (!TypeChecker.isInteger(row)) { throw Error(errorMsg) }
        if (!TypeChecker.isInteger(col)) { throw Error(errorMsg) }
        if (!TypeChecker.isInteger(grp)) { throw Error(errorMsg) }
        if (!TypeChecker.isInteger(state)) { throw Error(errorMsg) }

        if (idx < 0 || idx > 80) { throw Error(errorMsg) }
        if (row < 0 || row > 8) { throw Error(errorMsg) }
        if (col < 0 || col > 8) { throw Error(errorMsg) }
        if (grp < 0 || grp > 8) { throw Error(errorMsg) }
        if (state < 0 || state > 1) { throw Error(errorMsg) }

        const foundCell = this.getCellByIndex(idx);
        if (elem !== foundCell) { throw Error(errorMsg) }
    }
    /**
     * Toggles the cell on/off
     * @param {HTMLElement} cell 
     */
    toggleCell(cell) {
        this.assertIsCell(cell);
        let state = parseInt(cell.getAttribute("state"));
        state = 1 * (state === 0); // branchless flip
        cell.setAttribute("state", state);
    }
    /**
     * Returns the cells that would be filled if a shape would be placed at a 1D board-cell index (idx attribute on cell element)
     * Warning! Does not validate that the shape can be placed at the index, and may return fewer cells than the shape needs
     * @param {Number} shapeId Id of the shape to check
     * @param {Number} index 1D board-cell index (idx attribute on cell element)
     */
    getIntersectingCells(shapeId, index) {
        assertValidShapeId(shapeId);
        const shape = shapes[shapeId];
        const idx2D = indexTo2D(index, true);
        const cells = []
        let row = -99;
        let col = -99;
        for (let i = 0; i < shape.length; i++) {
            row = shape[i].r + idx2D.row;
            col = shape[i].c + idx2D.col;
            if (row < 0 || row > 8 || col < 0 || col > 8) { continue }
            cells.push(this.getCellByRowCol(row, col));
        }
        return cells;
    }
    //#endregion
    //#endregion

    //#region pieces
    get piece1() { return this.shadowRoot.getElementById('piece-1') }
    get piece2() { return this.shadowRoot.getElementById('piece-2') }
    get piece3() { return this.shadowRoot.getElementById('piece-3') }
    get pieces() { return [this.piece1, this.piece2, this.piece3] }

    /**
     * Sets a piece to the given shape
     * @param {string} pieceId css id of the piece to place from the piece-buffer
     * @param {Number} shapeId Id of the shape to check
     */
    setPieceShape(pieceId, shapeId) {
        assertValidShapeId(shapeId, true)
        const piece = this.shadowRoot.getElementById(pieceId);
        piece.setAttribute("shapeId", shapeId);
        if (shapeId === -1) { return }
        // Only render non-empty shapes
        const shapeRender = renderShape(shapeId);
        piece.style.backgroundImage = `url(${urlEncodeSvg(shapeRender)})`;
    }
    /**
     * Refills the piece buffer with 3 new pieces. Does not check if it's overwriting not-spent pieces
     */
    refill_piece_buffer() {
        // TODO Log this in replay
        const shapeId_1 = this.rand.nextInt() % shapeIds.length;
        const shapeId_2 = this.rand.nextInt() % shapeIds.length;
        const shapeId_3 = this.rand.nextInt() % shapeIds.length;

        this.setPieceShape('piece-1', shapeId_1);
        this.setPieceShape('piece-2', shapeId_2);
        this.setPieceShape('piece-3', shapeId_3);
    }
    /**
     * Refills the piece buffer if it's empty
     */
    refill_piece_buffer_if_empty() {
        const id1 = parseInt(this.piece1.getAttribute("shapeId"));
        const id2 = parseInt(this.piece2.getAttribute("shapeId"));
        const id3 = parseInt(this.piece3.getAttribute("shapeId"));
        if (id1 === -1 && id2 === -1 && id3 === -1) {
            this.refill_piece_buffer();
        }
    }
    //#endregion

    //#region Drag 'n Drop
    gameSelectedPiece = undefined;
    gameSelectedShapeId = undefined;
    gameSelectedShape = undefined;
    gameSelectedGrab = undefined;
    gameIntersectingCells = [];

    /**
     * Places this.gameSelectedPiece by using the information in this.gameIntersectingCells.
     * Warning! does not check validity of placement
     */
    placeSelectedPiece() {
        console.debug(this.localName, "placeSelectedPiece()")
        const pieceId = this.gameSelectedPiece.id;
        const index = this.getSelectedPieceHoveringIndex();
        this.clearSelectedPiece();
        this.tryPlacePiece(pieceId, index)
    }
    /**
     * Clears and resets this.gameSelectedPiece
     */
    clearSelectedPiece() {
        this.gameSelectedPiece.style.top = '';
        this.gameSelectedPiece.style.left = '';
        this.gameSelectedPiece.style.width = '';
        this.gameSelectedPiece.classList.remove('dragging');
        this.gameSelectedPiece = undefined;
        this.gameSelectedShapeId = undefined;
        this.gameSelectedShape = undefined;
    }
    /**
     * Sets the piece as the currently selected piece (the one the user is dragging around trying to place).
     * Overwrites information in this.gameSelectedPiece, this.gameSelectedShapeId, this.gameSelectedShape and this.gameIntersectingCells
     * @param {HTMLElement} piece
     */
    selectPiece(piece, grab_top = null, grab_left = null) {
        this.gameSelectedPiece = piece;
        this.gameSelectedPiece.classList.add('dragging');
        this.gameSelectedShapeId = parseInt(this.gameSelectedPiece.getAttribute("shapeId"));
        assertValidShapeId(this.gameSelectedShapeId);
        this.gameSelectedShape = shapes[this.gameSelectedShapeId]
        this.gameIntersectingCells.length = 0;

        const pieceBounds = this.gameSelectedPiece.getBoundingClientRect();
        grab_top = grab_top ?? pieceBounds.height / 2;
        grab_left = grab_left ?? pieceBounds.width / 2;
        this.gameSelectedGrab = { Y: grab_top, X: grab_left }
    }
    /**
     * Calculates the 1D board-cell index (idx attribute on cell element) that the currently selected piece (this.gameSelectedPiece) is hovering over
     * Warning! Can be out of bounds
     * @returns The calculated 1D board-cell index (idx attribute on cell element)
     */
    getSelectedPieceHoveringIndex() {
        if (this.gameSelectedPiece === undefined) { return; }
        const pieceBounds = this.gameSelectedPiece.getBoundingClientRect();
        const boardBounds = this.board.getBoundingClientRect();
        let col = rangeMapNumber(pieceBounds.x, boardBounds.left, boardBounds.right, 0, 9);
        let row = rangeMapNumber(pieceBounds.y, boardBounds.top, boardBounds.bottom, 0, 9);

        // Round towards board
        if (col < 0) { col = Math.ceil(col); }
        else if (col > 8) { col = Math.floor(col); }
        else { col = Math.round(col); }

        if (row < 0) { row = Math.ceil(row); }
        else if (row > 8) { row = Math.floor(row); }
        else { row = Math.round(row); }

        return indexTo1D(row, col, true);
    }
    /**
     * returns all the cells that intersect the currently selected piece (this.gameSelectedPiece)
     */
    getSelectedPieceIntersectingCells() {
        if (this.gameSelectedPiece === undefined) { return; }
        const index = this.getSelectedPieceHoveringIndex();

        return this.getIntersectingCells(this.gameSelectedShapeId, index);
    }
    /** * Clears highlight of every cell in this.gameIntersectingCells, and empties it */
    clearSelectedCells() {
        while (this.gameIntersectingCells.length > 0) {
            const cell = this.gameIntersectingCells.pop();
            cell.classList.remove("highlight");
        }
    }
    /** * Updates cell highlighting and the content of this.gameIntersectingCells based on selected piece hovering position*/
    updateSelectedCells() {
        // clear old intersection
        this.clearSelectedCells();

        const intersectingCells = this.getSelectedPieceIntersectingCells();
        if (intersectingCells.length !== this.gameSelectedShape.length) { return; }
        let cell_state = -1;
        for (let i = 0; i < intersectingCells.length; i++) {
            cell_state = parseInt(intersectingCells[i].getAttribute("state"));
            if (cell_state !== 0) { return }
        }
        this.gameIntersectingCells.length = intersectingCells.length;
        for (let i = 0; i < intersectingCells.length; i++) {
            intersectingCells[i].classList.add("highlight");
            this.gameIntersectingCells[i] = intersectingCells[i];
        }
    }
    /**
     * Repositions the currently selected piece and updates highlighted cells
     * @param {Number} mouseY
     * @param {Number} mouseX 
     */
    updateSelectedPiecePosition(mouseY, mouseX) {
        if (this.gameSelectedPiece === undefined) { return }
        const top = mouseY - this.gameSelectedGrab.Y;
        const left = mouseX - this.gameSelectedGrab.X;
        this.gameSelectedPiece.style.top = top + 'px';
        this.gameSelectedPiece.style.left = left + 'px';
        this.updateSelectedCells()
    }

    // TOUCH
    async piece_touchstart(event) {
        console.debug("piece_touchstart", "\n\tthis:", this, "\n\event.target:", event.target);

        const targetBounds = event.target.getBoundingClientRect();
        const targetTouch = event.targetTouches[0];
        const grab_left = rangeMapNumber(targetTouch.clientX, targetBounds.left, targetBounds.right, 0.0, 1.0) * targetBounds.width;
        const grab_top = rangeMapNumber(targetTouch.clientY, targetBounds.top, targetBounds.bottom, 0.0, 1.0) * targetBounds.height;

        this.selectPiece(event.target, grab_top, grab_left);
        console.log("piece_touchstart", "\n\tevent:", event, "\n\ttouch:", {
            target: event.target,
            targetBounds: event.target.getBoundingClientRect(),
            clientX: event.targetTouches[0].clientX,
            clientY: event.targetTouches[0].clientY,
            pageX: event.targetTouches[0].pageX,
            pageY: event.targetTouches[0].pageY,
            radiusX: event.targetTouches[0].radiusX,
            radiusY: event.targetTouches[0].radiusY,
            screenX: event.targetTouches[0].screenX,
            screenY: event.targetTouches[0].screenY,
        });

        this.gameSelectedPiece.addEventListener("touchmove", e => this.piece_touchmove(e), { passive: true });
        this.gameSelectedPiece.addEventListener("touchend", e => this.piece_touchend(e), { passive: true });
        this.gameSelectedPiece.addEventListener("touchcancel", e => this.piece_touchcancel(e), { passive: true });
        await this.piece_touchmove(event);
    }
    /**
     * @param {TouchEvent} event 
     */
    async piece_touchmove(event) {
        if (this.gameSelectedPiece === undefined || event.target !== this.gameSelectedPiece) { return; }

        const gameBounds = this.getBoundingClientRect();
        const touchpos = event.touches[0];
        const mouseX = touchpos.clientX - gameBounds.x;
        const mouseY = touchpos.clientY - gameBounds.y;
        await this.updateSelectedPiecePosition(mouseY, mouseX);
    }
    /**
     * @param {TouchEvent} event 
     */
    async piece_touchend(event) {
        if (this.gameSelectedPiece === undefined || event.target !== this.gameSelectedPiece) { return; }
        console.debug("piece_touchend", "\n\tthis:", this, "\n\event.target:", event.target);
        this.gameSelectedPiece.removeEventListener("touchmove", e => this.piece_touchmove(e), { passive: true });
        this.gameSelectedPiece.removeEventListener("touchend", e => this.piece_touchend(e), { passive: true });
        this.gameSelectedPiece.removeEventListener("touchcancel", e => this.piece_touchend(e), { passive: true });

        // const gameBounds = this.getBoundingClientRect();
        // const touchpos = event.touches[0];
        // const mouseX = touchpos.clientX - gameBounds.x;
        // const mouseY = touchpos.clientY - gameBounds.y;
        // await this.updateSelectedPiecePosition(mouseY, mouseX);

        await this.placeSelectedPiece();
    }
    /**
     * @param {TouchEvent} event 
     */
    async piece_touchcancel(event) {
        if (this.gameSelectedPiece === undefined || event.target !== this.gameSelectedPiece) { return; }
        const boardBounds = this.board.getBoundingClientRect();
        // Move the piece WAY outside the board, and THEN do the touchend
        await this.updateSelectedPiecePosition(boardBounds.bottom * 2, boardBounds.right * 2);
        await this.piece_touchend(event);
    }

    // POINTER
    currentDragPointer = undefined; // pointer id to currently dragging pointer
    async piece_pointerdown(event) {
        console.debug("piece_pointerdown", "\n\tthis:", this, "\n\event.target:", event.target);
        await this.selectPiece(event.target);
        this.currentDragPointer = event.pointerId;
        window.addEventListener("pointermove", e => this.window_pointermove(e), true);
        window.addEventListener("pointerup", e => this.window_pointerup(e), true);
    }
    /**
     * 
     * @param {DragEvent} event 
     */
    async window_pointermove(event) {
        if (this.gameSelectedPiece === undefined || event.pointerId !== this.currentDragPointer) { return; }
        const gameBounds = this.getBoundingClientRect();
        const mouseX = event.clientX - gameBounds.x;
        const mouseY = event.clientY - gameBounds.y;
        await this.updateSelectedPiecePosition(mouseY, mouseX);
    }
    /**
     * 
     * @param {DragEvent} event 
     */
    async window_pointerup(event) {
        if (this.gameSelectedPiece === undefined || event.pointerId !== this.currentDragPointer) { return; }
        console.debug("window_pointerup", "\n\tthis:", this, "\n\event.target:", event.target);
        window.removeEventListener("pointerup", e => this.window_pointerup(e), true);
        window.removeEventListener("pointermove", e => this.window_pointermove(e), true);

        const gameBounds = this.getBoundingClientRect();
        const mouseX = event.clientX - gameBounds.x;
        const mouseY = event.clientY - gameBounds.y;
        await this.updateSelectedPiecePosition(mouseY, mouseX)

        this.placeSelectedPiece();
    }
    //#endregion

    //#region Initialization
    initPieces() {
        const pieces = this.pieces;
        if (user_can_hover()) {
            for (let i = 0; i < pieces.length; i++) {
                console.debug("init piece:", pieces[i]);
                pieces[i].addEventListener("pointerdown", e => this.piece_pointerdown(e));
            }
        } else {
            for (let i = 0; i < pieces.length; i++) {
                console.debug("init piece:", pieces[i]);
                pieces[i].addEventListener("touchstart", e => this.piece_touchstart(e), { passive: true });
            }
        }
    }
    init_called = false;
    /**Initializes event handlers and more*/
    init() {
        if (this.init_called) { return; }
        this.startButton.addEventListener("click", e => this.startButton_click(e));
        this.resumeButton.addEventListener("click", e => this.reloadButton_click(e));
        this.resumeButton.setAttribute("canLoad", this.can_load());
        this.initPieces();
        this.init_called = true;
    }

    connectedCallback() {
        console.debug(this.localName, "connectedCallback()");
        let shadowRoot = this.attachShadow({ mode: "open" });
        shadowRoot.adoptedStyleSheets = [game_css];
        shadowRoot.appendChild(this.template.content.cloneNode(true));
        this.init()
    }

    constructor() {
        super();
        this.template = loadTemplate();
    }
    //#endregion
}

customElements.define(GameElementTagName, GameElement);
