export { GameElement, user_can_hover }
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
 * @returns The result 1D index in the range [0-80]
 */
function indexTo1D(row, col) {
    TypeChecker.assertIsInteger(row);
    TypeChecker.assertIsInteger(col);
    if (row < 0 || row > 8) { throw Error("row must be in the range [0 - 8], but was: " + row) }
    if (col < 0 || col > 8) { throw Error("col must be in the range [0 - 8], but was: " + col) }
    const r = Math.max(0, Math.min(9, row));
    const c = Math.max(0, Math.min(9, col));
    return r * 9 + c;
}
/**
 * Converts 1D board index to 2D board index
 * @param {Number} index 
 * @returns The resulting 2D index row and col
 */
function indexTo2D(index) {
    TypeChecker.assertIsInteger(index);
    if (index < 0 || index > 80) { throw Error("index must be in the range [0 - 80], but was: " + index) }
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
const block_url = import.meta.resolve("./block-e.svg");
const block_data = syncGet(block_url);
const block_data_url = `data:image/svg+xml;base64,${btoa(block_data)}`
const shape_template_url = import.meta.resolve("./shape_template.svg");
const shape_template_string = syncGet(shape_template_url).replaceAll('{block_url}', block_data_url);
const shape_block_string_template = `<rect x="{x}" y="{y}" width="10" height="10" fill="url(#block)" />`
const shapeRenderCache = {};
function renderShape(shapeId) {
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
//#endregion



// -- GameElement --
const GameElementTagName = 'game-wrap';
class GameElement extends HTMLElement {
    seed = prng.sfc32.getSeed();
    rand = new prng.sfc32(this.seed);
    restart() {
        if (!this.init_called) { this.init(); }
        console.debug(this.localName, "restart()");
        this.seed = prng.sfc32.getSeed();
        this.rand = new prng.sfc32(this.seed);
        this.refill_piece_buffer();

    }
    get startButton() {
        return this.shadowRoot.getElementById('start-button');
    }
    startButton_click(e) {
        console.debug("startButton_click");
        this.startButton.style.display = "none";
        this.restart();
    }

    //#region score
    set score(v) {
        // TODO
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

    /** clears all clearable cells. returns the delta score*/
    clear() {
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

        // DISABLING
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
    //#endregion
    //#endregion

    //#region pieces
    get piece1() { return this.shadowRoot.getElementById('piece-1') }
    get piece2() { return this.shadowRoot.getElementById('piece-2') }
    get piece3() { return this.shadowRoot.getElementById('piece-3') }
    get pieces() { return [this.piece1, this.piece2, this.piece3] }

    /**
     * 
     * @param {HTMLDivElement} piece 
     */
    fillPiece(piece) {
        const shapeId = this.rand.nextInt() % shapeIds.length;
        const shapeRender = renderShape(shapeId);
        const shape = shapes[shapeId];
        piece.setAttribute("shapeId", shapeId);
        piece.style.backgroundImage = `url(${urlEncodeSvg(shapeRender)})`;
        // TODO Write move log
    }
    /**
     * Refills the piece buffer with 3 new pieces. Does not check if it's overwriting not-spent pieces
     */
    refill_piece_buffer() {
        this.fillPiece(this.piece1);
        this.fillPiece(this.piece2);
        this.fillPiece(this.piece3);
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
    gameSelectedShapeId = -1;
    gameSelectedShape = undefined;
    gameIntersectingCells = [];
    placeSelectedPiece() {
        console.debug(this.localName, "placeSelectedPiece()")
        if (this.gameIntersectingCells.length > 0) {
            // TODO write move log
            while (this.gameIntersectingCells.length > 0) {
                // TODO Assert that the cells are all unset
                const cell = this.gameIntersectingCells.pop();
                cell.classList.remove('highlight');
                cell.setAttribute("state", 1);
                this.score++;
            }
            this.gameSelectedPiece.style = '';
            this.gameSelectedPiece.setAttribute("shapeId", -1);
            this.score += this.clear();
            this.refill_piece_buffer_if_empty();
        }
        this.clearSelectedPiece();
    }
    clearSelectedPiece() {
        this.gameSelectedPiece.style.top = '';
        this.gameSelectedPiece.style.left = '';
        this.gameSelectedPiece.style.width = '';
        this.gameSelectedPiece.classList.remove('dragging');
        this.gameSelectedPiece = undefined;
        this.gameSelectedShapeId = -1;
        this.gameSelectedShape = undefined;
    }
    /**
     * Sets the piece as the currently selected piece (the one the user is dragging around trying to place)
     * @param {HTMLElement} piece 
     */
    selectPiece(piece) {
        this.gameSelectedPiece = piece;
        this.gameSelectedPiece.classList.add('dragging');
        this.gameSelectedShapeId = parseInt(this.gameSelectedPiece.getAttribute("shapeId"));
        this.gameSelectedShape = shapes[this.gameSelectedShapeId]
    }
    /**
     * returns all the cells that intersect the currently selected piece
     */
    async getIntersectingCells() {
        if (this.gameSelectedPiece === undefined) { return; }
        const pieceBounds = this.gameSelectedPiece.getBoundingClientRect();
        const boardBounds = this.board.getBoundingClientRect();
        const pCol = Math.round(rangeMapNumber(pieceBounds.x, boardBounds.left, boardBounds.right, 0, 9));
        const pRow = Math.round(rangeMapNumber(pieceBounds.y, boardBounds.top, boardBounds.bottom, 0, 9));

        let intersectingCells = []
        for (let i = 0; i < this.gameSelectedShape.length; i++) {
            const offset = this.gameSelectedShape[i];
            const row = offset.r + pRow;
            const col = offset.c + pCol;
            if (row < 0 || row > 8 || col < 0 || col > 8) { continue; }
            intersectingCells.push(this.getCellByRowCol(row, col));
        }

        return intersectingCells;
    }
    clearSelectedCells() {
        while (this.gameIntersectingCells.length > 0) {
            const cell = this.gameIntersectingCells.pop();
            cell.classList.remove("highlight");
        }
    }
    async updateSelectedCells() {
        // clear old intersection
        this.clearSelectedCells();

        const intersectingCells = (await this.getIntersectingCells()).filter(c => parseInt(c.getAttribute("state")) === 0);
        if (intersectingCells.length !== this.gameSelectedShape.length) { return; }
        for (let i = 0; i < intersectingCells.length; i++) {
            intersectingCells[i].classList.add("highlight");
            this.gameIntersectingCells.push(intersectingCells[i]);
        }
    }
    async updateSelectedPiecePosition(top, left) {
        this.gameSelectedPiece.style.top = top + 'px';
        this.gameSelectedPiece.style.left = left + 'px';
        await this.updateSelectedCells();
    }

    //#endregion
    //#region TouchEvent
    async piece_touchstart(event) {
        console.debug("piece_touchstart", "\n\tthis:", this, "\n\event.target:", event.target);
        this.selectPiece(event.target);
        this.gameSelectedPiece.addEventListener("touchmove", e => this.piece_touchmove(e), { passive: true });
        this.gameSelectedPiece.addEventListener("touchend", e => this.piece_touchend(e), { passive: true });
        this.gameSelectedPiece.addEventListener("touchcancel", e => this.piece_touchcancel(e), { passive: true });
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
        const pieceBounds = this.gameSelectedPiece.getBoundingClientRect();
        const centerX = pieceBounds.width / 2;
        const centerY = pieceBounds.height / 2;
        const top = (mouseY - centerY);
        const left = (mouseX - centerX);
        await this.updateSelectedPiecePosition(top, left);
    }
    /**
     * @param {TouchEvent} event 
     */
    async piece_touchend(event) {
        if (this.gameSelectedPiece === undefined || event.target !== this.gameSelectedPiece) { return; }
        console.log("piece_touchend", "\n\tthis:", this, "\n\event.target:", event.target);
        this.gameSelectedPiece.removeEventListener("touchmove", e => this.piece_touchmove(e), { passive: true });
        this.gameSelectedPiece.removeEventListener("touchend", e => this.piece_touchend(e), { passive: true });
        this.gameSelectedPiece.removeEventListener("touchcancel", e => this.piece_touchend(e), { passive: true });

        this.placeSelectedPiece();
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
    //#endregion
    //#region PointerEvent
    currentDragPointer = undefined;
    async piece_pointerdown(event) {
        console.debug("piece_pointerdown", "\n\tthis:", this, "\n\event.target:", event.target);
        this.selectPiece(event.target);
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
        const pieceBounds = this.gameSelectedPiece.getBoundingClientRect();
        const top = (mouseY - (pieceBounds.width / 2));
        const left = (mouseX - (pieceBounds.height / 2));
        await this.updateSelectedPiecePosition(top, left);
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
        const pieceBounds = this.gameSelectedPiece.getBoundingClientRect();
        const top = (mouseY - (pieceBounds.width / 2));
        const left = (mouseX - (pieceBounds.height / 2));
        await this.updateSelectedPiecePosition(top, left);
        this.placeSelectedPiece();
    }
    //#endregion
    //#endregion

    //#region Initialization
    initBoard() {
        const game = this;
        const cells = this.getCells();
        for (let i = 0; i < cells.length; i++) {
            // TODO Drag over and Drop events
        }
    }
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




    connectedCallback() {
        console.debug(this.localName, "connectedCallback()");

        let shadowRoot = this.attachShadow({ mode: "open" });
        shadowRoot.adoptedStyleSheets = [game_css];
        shadowRoot.appendChild(this.template.content.cloneNode(true));
        this.startButton.addEventListener("click", e => this.startButton_click(e));
        this.initPieces();
        this.initBoard();
        this.init_called = true;
    }

    constructor() {
        super();
        this.template = loadTemplate();
    }
    //#endregion
}

customElements.define(GameElementTagName, GameElement);
