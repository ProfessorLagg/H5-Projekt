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
    ctx.imageSmoothingEnabled = false;
    ctx.clearRect(0, 0, board_canvas_size, board_canvas_size);

    if (game_state.selectedPieceId < 0 || game_state.selectedPieceId > 2) { return }
    if (!pointer_data.bounds.intersects(board_bounds)) { return }

    // convert pointer position from viewport coordinates to drag canvas coordinates
    const pointer_uv_bounds = new DOMRect(
        (pointer_data.bounds.x - board_bounds.left) / board_bounds.width,
        (pointer_data.bounds.y - board_bounds.top) / board_bounds.height,
        pointer_data.width / board_bounds.width,
        pointer_data.height / board_bounds.height
    );

    const pixel_bounds = new DOMRect();
    pixel_bounds.width = cell_size * 5;
    pixel_bounds.height = cell_size * 5;
    pixel_bounds.x = (board_canvas_size * pointer_uv_bounds.x) - pixel_bounds.width / 2;
    pixel_bounds.y = (board_canvas_size * pointer_uv_bounds.y) - pixel_bounds.height / 2;

    const col = Math.round(rangeMapNumber(pixel_bounds.x, 0, board_canvas_size, 0, 8));
    const row = Math.round(rangeMapNumber(pixel_bounds.y, 0, board_canvas_size, 0, 8));
    pixel_bounds.x = col * cell_size;
    pixel_bounds.y = row * cell_size;
    
    const shapeId = game_state.selectedShapeId;
    const shapeImg = renderShape(
        shapeId, // shapeId
        cell_size, // cellSize
        highlight_img, // blockimg
        true, // centerX
        true // centerY
    );
    ctx.drawImage(
        shapeImg,
        pixel_bounds.x,
        pixel_bounds.y,
        pixel_bounds.width,
        pixel_bounds.height
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
    if (game_state.pieces[0] >= 0 && game_state.selectedPieceId != 0) {
        const img1 = renderShape(game_state.pieces[0], cell_size, block_img, true);
        ctx1.drawImage(img1, 0, 0, piece_canvas_size, piece_canvas_size);
    }

    const ctx2 = piece1.getContext("2d");
    ctx2.clearRect(0, 0, piece_canvas_size, piece_canvas_size);
    if (game_state.pieces[1] >= 0 && game_state.selectedPieceId != 1) {
        const img2 = renderShape(game_state.pieces[1], cell_size, block_img, true);
        ctx2.drawImage(img2, 0, 0, piece_canvas_size, piece_canvas_size);
    }

    const ctx3 = piece2.getContext("2d");
    ctx3.clearRect(0, 0, piece_canvas_size, piece_canvas_size);
    if (game_state.pieces[2] >= 0 && game_state.selectedPieceId != 2) {
        const img3 = renderShape(game_state.pieces[2], cell_size, block_img, true);
        ctx3.drawImage(img3, 0, 0, piece_canvas_size, piece_canvas_size);
    }
}
function draw_piecedrag() {
    const ctx = piecedrag_canvas.getContext("2d");
    ctx.imageSmoothingEnabled = false;
    ctx.clearRect(0, 0, piecedrag_canvas.width, piecedrag_canvas.height);

    if (game_state.selectedPieceId < 0 || game_state.selectedPieceId > 2) { return }

    // convert pointer position from viewport coordinates to drag canvas coordinates
    const pointer_uv_bounds = new DOMRect(
        (pointer_data.bounds.x - piecedrag_bounds.left) / piecedrag_bounds.width,
        (pointer_data.bounds.y - piecedrag_bounds.top) / piecedrag_bounds.height,
        pointer_data.width / piecedrag_bounds.width,
        pointer_data.height / piecedrag_bounds.height
    );

    const pixel_bounds = new DOMRect();

    pixel_bounds.width = cell_size * 5;
    pixel_bounds.height = cell_size * 5;
    pixel_bounds.x = (piecedrag_canvas.width * pointer_uv_bounds.x) - pixel_bounds.width / 2;
    pixel_bounds.y = (piecedrag_canvas.height * pointer_uv_bounds.y) - pixel_bounds.height / 2;

    const shapeId = game_state.selectedShapeId;
    const shapeImg = renderShape(
        shapeId, // shapeId
        cell_size, // cellSize
        block_img, // blockimg
        true, // centerX
        true // centerY
    );

    ctx.drawImage(
        shapeImg,
        pixel_bounds.x,
        pixel_bounds.y,
        pixel_bounds.width,
        pixel_bounds.height
    );

    // ctx.fillStyle = "rgba(255,0,0,.1)"
    // ctx.fillRect(
    //     pixel_bounds.x,
    //     pixel_bounds.y,
    //     pixel_bounds.width,
    //     pixel_bounds.height
    // );


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
    draw_board_highlight();
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
const pointer_data = new PointerData();
async function pointermoveHandler(event) {
    if (!pointer_data.update(event)) { return }
    if (game_state.selectedPieceId < 0 || game_state.selectedPieceId > 2) { return }
    requestAnimationFrame(update_piecedrag);
}
function pointerdownHandler(event) {
    if (!pointer_data.update(event)) { return }
    console.debug("pointerdown");
    if (game_state.pieces[0] >= 0 && pointer_data.bounds.intersects(piece0_bounds)) {
        game_state.selectPiece(0);
        console.log("game_state.selectedPieceId:", game_state.selectedPieceId)
        gameUpdate();
    } else if (game_state.pieces[1] >= 0 && pointer_data.bounds.intersects(piece1_bounds)) {
        game_state.selectPiece(1);
        requestAnimationFrame(gameUpdate);
    } else if (game_state.pieces[2] >= 0 && pointer_data.bounds.intersects(piece2_bounds)) {
        game_state.selectPiece(2);
        requestAnimationFrame(gameUpdate);
    }
}
function pointerupHandler(event) {
    if (!pointer_data.update(event)) { return }
    console.debug("pointerup");
    // TODO Check if i can place the currently selected piece

    game_state.clearSelectedPiece();
    gameUpdate();
}


