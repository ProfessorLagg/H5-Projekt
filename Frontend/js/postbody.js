const game_wrap = document.getElementById('game-wrap');
const board_background = document.getElementById('board-background');
const board_cells = document.getElementById('board-cells');
const board_highlight = document.getElementById('board-highlight');
const board_borders = document.getElementById('board-borders');
const board_bounds = new DOMRect();

const block_img = document.getElementById('blck-img');
const cell_img = document.getElementById('cell-img');
const highlight_img = document.getElementById('highlight-img');

const cell_size = 100;
const border_size = 5;
const board_canvas_size = (cell_size * 9) + (border_size * 11);
const group_size = board_canvas_size / 3;
const cellBounds = new Array(81);

const piece1 = document.getElementById('piece1');
const piece2 = document.getElementById('piece2');
const piece3 = document.getElementById('piece3');
const piece1_bounds = new DOMRect();
const piece2_bounds = new DOMRect();
const piece3_bounds = new DOMRect();

// utils
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
function indexToGroup(row, col) {
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
function roundDecimals(value, decimals) {
    const mul = Math.pow(10, Math.max(0, decimals));
    return Math.round(value * mul) / mul;
}

// draw functions
function draw_board_background() {
    const ctx = board_background.getContext("2d");
    ctx.fillStyle = "rgba(100,45,0,100%)";
    ctx.fillRect(0, 0, board_canvas_size, board_canvas_size);

    // Dark Groups
    ctx.fillStyle = "rgba(255,255,255,33%)";
    ctx.fillRect(0, 0, group_size, group_size);
    ctx.fillRect(group_size * 2, 0, group_size, group_size);
    ctx.fillRect(group_size, group_size, group_size, group_size);
    ctx.fillRect(0, group_size * 2, group_size, group_size);
    ctx.fillRect(group_size * 2, group_size * 2, group_size, group_size);

    // Light Groups
    ctx.fillStyle = "rgba(255,255,255,50%)";
    ctx.fillRect(group_size, 0, group_size, group_size);
    ctx.fillRect(0, group_size, group_size, group_size);
    ctx.fillRect(group_size * 2, group_size, group_size, group_size);
    ctx.fillRect(group_size, group_size * 2, group_size, group_size);
}
function draw_board_cells() {
    const ctx = board_cells.getContext("2d");
    ctx.clearRect(0, 0, board_canvas_size, board_canvas_size);
    for (let i = 0; i < cellBounds.length; i++) {
        const cell = cellBounds[i];
        ctx.drawImage(
            cell_img,
            Math.floor(cell.x),
            Math.floor(cell.y),
            Math.floor(cell.w),
            Math.floor(cell.h)
        );
    }
}
function draw_board_highlight() {
    const ctx = board_highlight.getContext("2d");
    ctx.clearRect(0, 0, board_canvas_size, board_canvas_size);
    if (pointermove_data.buttons <= 0) { return }


    const pointer_intersects_board = pointermove_data.bounds.intersects(board_bounds);
    if (!pointer_intersects_board) { return }
    const uvX = (pointermove_data.clientX - board_bounds.left) / board_bounds.width;
    const uvY = (pointermove_data.clientY - board_bounds.top) / board_bounds.height;

    // TODO Do this for every offset in the currently selected shape
    const col = Math.floor(rangeMapNumber(uvX, 0, 1, 0, 9));
    const row = Math.floor(rangeMapNumber(uvY, 0, 1, 0, 9));
    const idx = indexTo1D(row, col);

    ctx.fillStyle = "rgba(255,255,0,.5)"
    ctx.drawImage(
        highlight_img,
        cellBounds[idx].x,
        cellBounds[idx].y,
        cellBounds[idx].w,
        cellBounds[idx].h
    );

}
function draw_board_borders() {
    const ctx = board_borders.getContext("2d");
    ctx.clearRect(0, 0, board_canvas_size, board_canvas_size);
    for (let i = 0; i < cellBounds.length; i++) {
        if (!cellFilled(i)) { continue }

        const cell = cellBounds[i];
        ctx.drawImage(
            block_img,
            Math.floor(cell.x),
            Math.floor(cell.y),
            Math.floor(cell.w),
            Math.floor(cell.h)
        );
    }
}
function update() {
    draw_board_highlight();
}

function cellFilled(cellId) {
    // TODO make this not a debug version
    return Math.random() <= .33;
}
function updateBoardRect() {
    console.debug(arguments.callee.name)
    const curr_bounds = board_background.getBoundingClientRect();
    board_bounds.x = curr_bounds.x;
    board_bounds.y = curr_bounds.y;
    board_bounds.width = curr_bounds.width;
    board_bounds.height = curr_bounds.height;
}
function updatePieceRects() {
    const bounds1 = piece1.getBoundingClientRect();
    piece1_bounds.x = bounds1.x;
    piece1_bounds.y = bounds1.y;
    piece1_bounds.width = bounds1.width;
    piece1_bounds.height = bounds1.height;

    const bounds2 = piece2.getBoundingClientRect();
    piece2_bounds.x = bounds2.x;
    piece2_bounds.y = bounds2.y;
    piece2_bounds.width = bounds2.width;
    piece2_bounds.height = bounds2.height;

    const bounds3 = piece3.getBoundingClientRect();
    piece3_bounds.x = bounds3.x;
    piece3_bounds.y = bounds3.y;
    piece3_bounds.width = bounds3.width;
    piece3_bounds.height = bounds3.height;
}

// init
async function init() {
    console.log(arguments.callee.name);
    await loadShapes();
    initCellBounds();
    initBoardCanvas();
    initPointerEvents();
    initResizeEvent();
}
function initBoardCanvas() {
    console.log(arguments.callee.name);
    Array.from(document.querySelectorAll(".board-canvas")).forEach(board_canvas => {
        board_canvas.width = board_canvas_size;
        board_canvas.height = board_canvas_size;
    });
    draw_board_background();
    draw_board_cells();
    draw_board_highlight();
    draw_board_borders();
}
function initCellBounds() {
    console.log(arguments.callee.name);
    const s = board_canvas_size / 9;
    let i = 0;
    for (let r = 0; r < 9; r++) {
        for (let c = 0; c < 9; c++) {
            cellBounds[i] = {
                x: c * s,
                y: r * s,
                w: s,
                h: s
            };
            i++;
        }
    }
}
function initPointerEvents() {
    console.log(arguments.callee.name);
    window.addEventListener("pointermove", pointermoveHandler);
    window.addEventListener("pointerdown", pointerdownHandler);
    window.addEventListener("pointerup", pointerupHandler);
}
function initResizeEvent() {
    resizeHandler();
    window.addEventListener("resize", resizeHandler)
}

// resize event
async function resizeHandler() {
    updateBoardRect();
    updatePieceRects();
}

// pointer events
const pointermove_data = new PointerData();
const pointerdown_data = new PointerData();
const pointerup_data = new PointerData();
async function pointermoveHandler(event) {
    if (pointermove_data.update(event)) {
        // requestAnimationFrame(update);
        update();
    }
}
async function pointerdownHandler(event) {
    if (pointerdown_data.update(event)) {
        console.debug("pointerdown");
        requestAnimationFrame(update);
    }
}
async function pointerupHandler(event) {
    if (pointerup_data.update(event)) {
        console.debug("pointerup");
        requestAnimationFrame(update);
    }
}

// Game State
const boardState = new Uint8Array(81);
const pieceBufferState = new Uint16Array(3);
