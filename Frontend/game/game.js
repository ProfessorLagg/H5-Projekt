export { GameElement, user_can_hover }
// Imports https://html.spec.whatwg.org/multipage/webappapis.html#module-type-allowed
import { TypeChecker } from "./typechecker.mjs"
import * as prng from "./prng.mjs";
import game_css from "./game.css" with { type: "css" };


//#region utils
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
        console.debug(this.localName, "restart()");
        this.seed = prng.sfc32.getSeed();
        this.rand = new prng.sfc32(this.seed);
        this.fillPieceBuffer();
    }

    //#region Board

    get board() { return this.shadowRoot.getElementById('game-board') }
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
        piece.setAttribute("shapeId", shapeId);
        piece.childNodes.forEach(child => piece.removeChild(child));
        piece.appendChild(renderShape(shapeId).cloneNode(true));
    }
    fillPieceBuffer() {
        this.fillPiece(this.piece1);
        this.fillPiece(this.piece2);
        this.fillPiece(this.piece3);
    }
    //#endregion

    //#region Dragging
    currentDragPiece = undefined;
    /**
     * 
     * @param {DragEvent} event 
     */
    piece_mousedown(event) {
        let piece = event.target;
        console.debug("piece_mousedown", "\n\tthis:", this, "\n\event.target:", event.target);
        this.currentDragPiece = piece;
        window.addEventListener("mousemove", e => this.window_mousemove(e), true);
        window.addEventListener("mouseup", e => this.window_mouseup(e), true);
        this.currentDragPiece.classList.add('dragging');
    }
    /**
     * 
     * @param {DragEvent} event 
     */
    window_mousemove(event) {
        if (this.currentDragPiece === undefined) { return; }
        this.currentDragPiece.style.top = (event.clientY - this.currentDragPiece.clientHeight / 2) + 'px';
        this.currentDragPiece.style.left = (event.clientX - this.currentDragPiece.clientWidth / 2) + 'px';
        this.currentDragPiece.style.width = ((this.clientWidth / 9) * 5) + 'px';
    }
    /**
     * 
     * @param {DragEvent} event 
     */
    window_mouseup(event) {
        if (this.currentDragPiece === undefined) { return; }
        console.debug("window_mouseup", "\n\tthis:", this, "\n\event.target:", event.target);
        window.removeEventListener("mousemove", e => this.window_mousemove(e), true);
        window.removeEventListener("mouseup", e => this.window_mouseup(e), true);

        this.currentDragPiece.removeAttribute("style");
        this.currentDragPiece.classList.remove('dragging');
        this.currentDragPiece = undefined;
    }
    boardcell_dragover(event) {

    }

    // TODO Touch events
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
        for (let i = 0; i < pieces.length; i++) {
            console.debug("init piece:", pieces[i]);
            pieces[i].addEventListener("mousedown", e => this.piece_mousedown(e));
        }
    }

    connectedCallback() {
        console.debug(this.localName, "connectedCallback()");

        let shadowRoot = this.attachShadow({ mode: "open" });
        shadowRoot.adoptedStyleSheets = [game_css];

        shadowRoot.appendChild(this.template.content.cloneNode(true));
        this.initBoard();
        this.initPieces();
        this.restart();
    }

    constructor() {
        super();
        this.template = loadTemplate();

    }
    //#endregion
}

customElements.define(GameElementTagName, GameElement);
