export { GameElement, syncGet }
// Imports https://html.spec.whatwg.org/multipage/webappapis.html#module-type-allowed
import { TypeChecker } from "./typechecker.mjs"
import * as prng from "./prng.mjs";
import game_css from "./game.css" with { type: "css" };
import shapes from "./shapes.json" with { type: "json" };

// -- utils --
function syncGet(url) {
    let httpRequest = new XMLHttpRequest();
    httpRequest.open("GET", url, false);
    httpRequest.send();
    return httpRequest.response;
}
function indexTo1D(row, col) {
    const r = Math.max(0, Math.min(9, row));
    const c = Math.max(0, Math.min(9, col));
    return r * 9 + c;
}
function indexTo2D(index) {
    const idx = Math.max(0, Math.min(80, index));
    const col = idx % 9;
    const row = (idx - col) / 9
    return {
        row: row,
        col: col,
    }
}

// -- game template --
const template_url = import.meta.resolve("./game.html");
function loadTemplate() {
    console.debug("loadTemplate()");
    let txt = syncGet(template_url);
    let elem = new DOMParser().parseFromString(txt, "text/html").head.firstChild;
    elem.id = "tmpl-" + GameElementTagName;
    return elem;
}

// -- GameElement --
const GameElementTagName = 'game-wrap';
class GameElement extends HTMLElement {


    restart() {
        console.debug(this.localName, "restart()");
        this.seed = prng.sfc32.getSeed();
        this.rand = new prng.sfc32(this.seed);
    }

    //#region cell funcs
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
        if (!TypeChecker.isInteger(index)) { throw Error(`Expected integer, but found: ${index}`) }
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
        if (!TypeChecker.isInteger(row)) { throw Error(`Expected integer, but found: ${row}`) }
        if (!TypeChecker.isInteger(col)) { throw Error(`Expected integer, but found: ${col}`) }
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
        if (!TypeChecker.isInteger(row)) { throw Error(`Expected integer, but found: ${row}`) }
        if (row < 0 || row > 8) { throw Error("row must be in the range [0 - 8], but was: " + row) }
        return Array.from(this.shadowRoot.querySelectorAll(`.board-cell[row="${row}"]`));
    }

    /**
     * 
     * @param {Number} col 
     * @returns All the cells in the specified column
     */
    getColumn(col) {
        if (!TypeChecker.isInteger(col)) { throw Error(`Expected integer, but found: ${col}`) }
        if (col < 0 || col > 8) { throw Error("col must be in the range [0 - 8], but was: " + col) }
        return Array.from(this.shadowRoot.querySelectorAll(`.board-cell[col="${col}"]`));
    }

    /**
     * 
     * @param {Number} grp 
     * @returns All the cells in the specified group
     */
    getGroup(grp) {
        if (!TypeChecker.isInteger(grp)) { throw Error(`Expected integer, but found: ${grp}`) }
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

    //#region Initialization
    initBoard() {
        const game = this;
        const cells = this.getCells();
        for (let i = 0; i < cells.length; i++) {
            cells[i].addEventListener("click", e => {
                game.toggleCell(e.target);
            });
        }
    }

    connectedCallback() {
        console.debug(this.localName, "connectedCallback()");

        let shadowRoot = this.attachShadow({ mode: "open" });
        shadowRoot.adoptedStyleSheets = [game_css];

        shadowRoot.appendChild(this.template.content.cloneNode(true));
        this.initBoard();
        this.restart();
    }

    constructor() {
        super();
        this.template = loadTemplate();
        this.seed = prng.sfc32.getSeed();
        this.rand = new prng.sfc32(this.seed);
    }
    //#endregion
}

customElements.define(GameElementTagName, GameElement);
