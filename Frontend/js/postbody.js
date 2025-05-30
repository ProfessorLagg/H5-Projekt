const game_wrap = document.getElementById('game-wrap');

const game_score = document.getElementById('game-score');

const board_background = document.getElementById('board-background');
const board_cells = document.getElementById('board-cells');
const board_highlight = document.getElementById('board-highlight');
const board_borders = document.getElementById('board-borders');
const board_bounds = new DOMRect();

const block_img = document.getElementById('blck-img');
const cell_img = document.getElementById('cell-img');
const highlight_img = document.getElementById('highlight-img');

const board_canvas_size = 990;
const group_size = board_canvas_size / 3;
const cell_size = board_canvas_size / 9;
const cellBounds = new Array(81);

const piece_canvas_size = (board_canvas_size / 9) * 5;
const piece0 = document.getElementById('piece0');
const piece1 = document.getElementById('piece1');
const piece2 = document.getElementById('piece2');
const piece0_bounds = new DOMRect();
const piece1_bounds = new DOMRect();
const piece2_bounds = new DOMRect();

const piecedrag_canvas = document.getElementById('piecedrag');
const piecedrag_bounds = new DOMRect();


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
function draw_board_state() {
    const ctx = board_borders.getContext("2d");
    ctx.clearRect(0, 0, board_canvas_size, board_canvas_size);
    for (let i = 0; i < cellBounds.length; i++) {
        if (!game_state.getCellState(i)) { continue }

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
function draw_piecebuffer() {
    const ctx1 = piece0.getContext("2d");
    ctx1.clearRect(0, 0, piece_canvas_size, piece_canvas_size);
    if (game_state.pieces[0] >= 0 && game_state.selectedPiece != 0) {
        const img1 = renderShape(game_state.pieces[0], cell_size, block_img);
        ctx1.drawImage(img1, 0, 0, piece_canvas_size, piece_canvas_size);
    }

    const ctx2 = piece1.getContext("2d");
    ctx2.clearRect(0, 0, piece_canvas_size, piece_canvas_size);
    if (game_state.pieces[1] >= 0 && game_state.selectedPiece != 1) {
        const img2 = renderShape(game_state.pieces[1], cell_size, block_img);
        ctx2.drawImage(img2, 0, 0, piece_canvas_size, piece_canvas_size);
    }

    const ctx3 = piece2.getContext("2d");
    ctx3.clearRect(0, 0, piece_canvas_size, piece_canvas_size);
    if (game_state.pieces[2] >= 0 && game_state.selectedPiece != 2) {
        const img3 = renderShape(game_state.pieces[2], cell_size, block_img);
        ctx3.drawImage(img3, 0, 0, piece_canvas_size, piece_canvas_size);
    }
}
function draw_piecedrag() {
    const ctx = piecedrag_canvas.getContext("2d");
    ctx.clearRect(0, 0, piecedrag_canvas.width, piecedrag_canvas.height);
    if (game_state.selectedPiece < 0 || game_state.selectedPiece > 2) { return }

    // TODO convert pointer position from viewport coordinates to drag canvas coordinates
    const pointer_uv_bounds = pointermove_data.bounds.uv();
    const pixel_bounds = new DOMRect();
    pixel_bounds.x = piecedrag_canvas.width * pointer_uv_bounds.x;
    pixel_bounds.y = piecedrag_canvas.height * pointer_uv_bounds.y;
    pixel_bounds.width = cell_size * 5;
    pixel_bounds.height = cell_size * 5;

    const shapeImg = renderShape(game_state.pieces[game_state.selectedPiece], cell_size, block_img);
    ctx.drawImage(
        shapeImg,
        pixel_bounds.x,
        pixel_bounds.y,
        pixel_bounds.width,
        pixel_bounds.height
    );
}
function draw_score() {
    game_score.innerText = game_state.score;
}

// Game
let game_state = new GameState();
/**Updates and redraws the board. Intended to be run via. requestAnimationFrame */
function update_board(timestamp = -1) {
    draw_board_highlight();
    draw_board_state();
}
/**Updates and redraws the piece buffer. Intended to be run via. requestAnimationFrame */
function update_piecebuffer(timestamp = -1) {
    draw_piecebuffer();
}
/**Updates and redraws the piece drag canvas. Intended to be run via. requestAnimationFrame */
function update_piecedrag(timestamp = -1) {
    draw_piecedrag();
}
/**Updates and redraws the score. Intended to be run via. requestAnimationFrame */
function update_score(timestamp = -1) {
    draw_score();
}
/**Full, expensive, game update. Intended to be run via. requestAnimationFrame */
function gameUpdate(timestamp = -1) {
    update_board(timestamp);
    update_piecebuffer(timestamp);
    update_piecedrag(timestamp);
    update_score(timestamp);
}


// init
async function init() {
    console.log(arguments.callee.name);
    await loadShapes();
    await initCellBounds();
    await initBoardCanvas();
    await initPieceCanvas();
    await initGameState(); // MUST BE RUN AFTER initPieceCanvas()
    await initPointerEvents();
    await initResizeEvent();
}
async function initBoardCanvas() {
    console.log(arguments.callee.name);
    Array.from(document.querySelectorAll(".board-canvas")).forEach(board_canvas => {
        board_canvas.width = board_canvas_size;
        board_canvas.height = board_canvas_size;
    });
    draw_board_background();
    draw_board_cells();
    draw_board_highlight();
    draw_board_state();
}
async function initPieceCanvas() {
    piece0.width = piece_canvas_size;
    piece0.height = piece_canvas_size;
    piece1.width = piece_canvas_size;
    piece1.height = piece_canvas_size;
    piece2.width = piece_canvas_size;
    piece2.height = piece_canvas_size;
}
async function initCellBounds() {
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
async function initPointerEvents() {
    console.log(arguments.callee.name);
    window.addEventListener("pointermove", pointermoveHandler);
    window.addEventListener("pointerdown", pointerdownHandler);
    window.addEventListener("pointerup", pointerupHandler);
}
async function initResizeEvent() {
    resizeHandler();
    window.addEventListener("resize", resizeHandler)
}
async function initGameState() {
    game_state.boardStateChangedCallback = () => requestAnimationFrame(update_board);
    game_state.pieceBufferChangedCallback = () => requestAnimationFrame(update_piecebuffer);
    game_state.scoreChangedCallback = () => requestAnimationFrame(update_score);
    game_state.restart();
    // requestAnimationFrame(gameUpdate);
}

// resize event
function updateBoundsData() {
    console.debug(arguments.callee.name)
    const c_board_bounds = board_background.getBoundingClientRect();
    board_bounds.x = c_board_bounds.x;
    board_bounds.y = c_board_bounds.y;
    board_bounds.width = c_board_bounds.width;
    board_bounds.height = c_board_bounds.height;

    const c_piece0_bounds = piece0.getBoundingClientRect();
    piece0_bounds.x = c_piece0_bounds.x;
    piece0_bounds.y = c_piece0_bounds.y;
    piece0_bounds.width = c_piece0_bounds.width;
    piece0_bounds.height = c_piece0_bounds.height;

    const c_piece1_bounds = piece1.getBoundingClientRect();
    piece1_bounds.x = c_piece1_bounds.x;
    piece1_bounds.y = c_piece1_bounds.y;
    piece1_bounds.width = c_piece1_bounds.width;
    piece1_bounds.height = c_piece1_bounds.height;

    const c_piece2_bounds = piece2.getBoundingClientRect();
    piece2_bounds.x = c_piece2_bounds.x;
    piece2_bounds.y = c_piece2_bounds.y;
    piece2_bounds.width = c_piece2_bounds.width;
    piece2_bounds.height = c_piece2_bounds.height;

    const c_piecedrag_bounds = piecedrag_canvas.getBoundingClientRect();
    piecedrag_bounds.x = c_piecedrag_bounds.x;
    piecedrag_bounds.y = c_piecedrag_bounds.y;
    piecedrag_bounds.width = c_piecedrag_bounds.width;
    piecedrag_bounds.height = c_piecedrag_bounds.height;
}
function update_piecedrag_size() {
    console.debug(arguments.callee.name)
    const scaleW = piecedrag_bounds.width / board_bounds.width;
    const scaleH = piecedrag_bounds.height / board_bounds.height;
    piecedrag_canvas.width = board_canvas_size * scaleW;
    piecedrag_canvas.height = board_canvas_size * scaleH;
}
async function resizeHandler() {
    updateBoundsData();
    update_piecedrag_size();
}

// pointer events
const pointermove_data = new PointerData();
const pointerdown_data = new PointerData();
const pointerup_data = new PointerData();
async function pointermoveHandler(event) {
    if (!pointermove_data.update(event)) { return }
    if (game_state.selectedPiece < 0 || game_state.selectedPiece > 2) { return }
    requestAnimationFrame(() => {
        update_piecedrag();
        update_board();
    });
}
async function pointerdownHandler(event) {
    if (!pointerdown_data.update(event)) { return }
    console.debug("pointerdown");
    if (game_state.pieces[0] >= 0 && pointerdown_data.bounds.intersects(piece0_bounds)) {
        game_state.selectPiece(0);
    } else if (game_state.pieces[1] >= 0 && pointerdown_data.bounds.intersects(piece1_bounds)) {
        game_state.selectPiece(1);
    } else if (game_state.pieces[2] >= 0 && pointerdown_data.bounds.intersects(piece2_bounds)) {
        game_state.selectPiece(2);
    }
    requestAnimationFrame(gameUpdate);
}
async function pointerupHandler(event) {
    if (!pointerup_data.update(event)) { return }
    _ = pointermove_data.update(event);
    console.debug("pointerup");
    // TODO Check if i can place the currently selected piece

    game_state.clearSelectedPiece();
    requestAnimationFrame(gameUpdate);
}


