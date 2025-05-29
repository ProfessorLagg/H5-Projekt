const game_wrap = document.getElementById('game-wrap');
const board_background = document.getElementById('board-background');
const board_cells = document.getElementById('board-cells');
const board_highlight = document.getElementById('board-highlight');
const board_borders = document.getElementById('board-borders');
const board_bounds = new DOMRect(); // TODO update this on resize

const block_img = document.getElementById('blck-img');
const cell_img = document.getElementById('cell-img');

const cell_size = 100;
const border_size = 5;
const board_canvas_size = (cell_size * 9) + (border_size * 11);
const group_size = board_canvas_size / 3;
const outerCellBounds = new Array(81);

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
//#endregion

//#region draw functions
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
    for (let i = 0; i < outerCellBounds.length; i++) {
        const cell = outerCellBounds[i];
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
}
function draw_board_borders() {
    const ctx = board_borders.getContext("2d");
    ctx.clearRect(0, 0, board_canvas_size, board_canvas_size);
    for (let i = 0; i < outerCellBounds.length; i++) {
        if(!cellFilled(i)){continue}

        const cell = outerCellBounds[i];
        ctx.drawImage(
            block_img,
            Math.floor(cell.x),
            Math.floor(cell.y),
            Math.floor(cell.w),
            Math.floor(cell.h)
        );
    }
}

function update(deltaTime) {
    draw_board_highlight();
}
//#endregion


const boardState = new Uint8Array(81);
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

//#region init
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
    // draw_board_highlight();
    draw_board_borders();

    updateBoardRect();
}
function initCellBounds() {
    console.log(arguments.callee.name);
    const s = board_canvas_size / 9;
    let i = 0;
    for (let r = 0; r < 9; r++) {
        for (let c = 0; c < 9; c++) {
            outerCellBounds[i] = {
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
    window.addEventListener("resize", resizeHandler)
}
//#endregion

//#region resize event
async function resizeHandler(event) {
    updateBoardRect();
}

//#region pointer events
const pointermove_data = {
    /**time (in milliseconds) at which the event was created. */
    timeStamp: -1,
    /**Event type */
    type: "",
    /**The X coordinate of the pointer in viewport coordinates. */
    clientX: -1,
    /**The Y coordinate of the pointer in viewport coordinates. */
    clientY: -1,
    /**The X coordinate of the pointer relative to the position of the padding edge of the target node. */
    offsetX: -1,
    /**The Y coordinate of the pointer relative to the position of the padding edge of the target node. */
    offsetY: -1,
    /**The X coordinate of the pointer relative to the whole document. */
    pageX: -1,
    /**The X coordinate of the pointer relative to the whole document. */
    pageY: -1,
    /**The X coordinate of the pointer in screen coordinates. */
    screenX: -1,
    /**The X coordinate of the pointer in screen coordinates. */
    screenY: -1,
    /**The X coordinate of the pointer relative to the position of the last move event. */
    movementX: -1,
    /**The Y coordinate of the pointer relative to the position of the last move event. */
    movementY: -1,
    /**The width (magnitude on the X axis), in CSS pixels, of the contact geometry of the pointer. */
    width: -1,
    /**The height (magnitude on the Y axis), in CSS pixels, of the contact geometry of the pointer. */
    height: -1,
    /**Represents the angle between a transducer (a pointer or stylus) axis and the X-Y plane of a device screen. */
    altitudeAngle: -1,
    /**Represents the angle between the Y-Z plane and the plane containing both the transducer (a pointer or stylus) axis and the Y axis. */
    azimuthAngle: -1,
    /**The button number that was pressed or released (if applicable) when the mouse event was fired. */
    button: -1,
    /**The buttons being pressed (if any) when the event was fired. */
    buttons: -1,
    /**Normalized pressure of the pointer input in the range of 0 to 1, where 0 and 1 represent the minimum and maximum pressure the hardware is capable of detecting, respectively. */
    pressure: -1,
    /**Normalized tangential pressure of the pointer input (also known as barrel pressure or cylinder stress) in the range -1 to 1, where 0 is the neutral position of the control. */
    tangentialPressure: -1,
    /**Plane angle (in degrees, in the range of -90 to 90) between the Y–Z plane and the plane containing both the pointer (e.g., pen stylus) axis and the Y axis. */
    tiltX: -1,
    /**Plane angle (in degrees, in the range of -90 to 90) between the X–Z plane and the plane containing both the pointer (e.g., pen stylus) axis and the X axis. */
    tiltY: -1,
    /**A unique identifier for the pointing device generating the PointerEvent. */
    persistentDeviceId: -1,
    /**A unique identifier for the pointer causing the event. */
    pointerId: -1,
    /**Indicates the device type that caused the event (mouse, pen, touch, etc.). */
    pointerType: "",
    /**Indicates if the pointer represents the primary pointer of this pointer type. */
    isPrimary: false,
}
const pointermove_keys = Object.keys(pointermove_data);
const pointerdown_data = {
    /**time (in milliseconds) at which the event was created. */
    timeStamp: -1,
    /**Event type */
    type: "",
    /**The X coordinate of the pointer in viewport coordinates. */
    clientX: -1,
    /**The Y coordinate of the pointer in viewport coordinates. */
    clientY: -1,
    /**The X coordinate of the pointer relative to the position of the padding edge of the target node. */
    offsetX: -1,
    /**The Y coordinate of the pointer relative to the position of the padding edge of the target node. */
    offsetY: -1,
    /**The X coordinate of the pointer relative to the whole document. */
    pageX: -1,
    /**The X coordinate of the pointer relative to the whole document. */
    pageY: -1,
    /**The X coordinate of the pointer in screen coordinates. */
    screenX: -1,
    /**The X coordinate of the pointer in screen coordinates. */
    screenY: -1,
    /**The X coordinate of the pointer relative to the position of the last move event. */
    movementX: -1,
    /**The Y coordinate of the pointer relative to the position of the last move event. */
    movementY: -1,
    /**The width (magnitude on the X axis), in CSS pixels, of the contact geometry of the pointer. */
    width: -1,
    /**The height (magnitude on the Y axis), in CSS pixels, of the contact geometry of the pointer. */
    height: -1,
    /**Represents the angle between a transducer (a pointer or stylus) axis and the X-Y plane of a device screen. */
    altitudeAngle: -1,
    /**Represents the angle between the Y-Z plane and the plane containing both the transducer (a pointer or stylus) axis and the Y axis. */
    azimuthAngle: -1,
    /**The button number that was pressed or released (if applicable) when the mouse event was fired. */
    button: -1,
    /**The buttons being pressed (if any) when the event was fired. */
    buttons: -1,
    /**Normalized pressure of the pointer input in the range of 0 to 1, where 0 and 1 represent the minimum and maximum pressure the hardware is capable of detecting, respectively. */
    pressure: -1,
    /**Normalized tangential pressure of the pointer input (also known as barrel pressure or cylinder stress) in the range -1 to 1, where 0 is the neutral position of the control. */
    tangentialPressure: -1,
    /**Plane angle (in degrees, in the range of -90 to 90) between the Y–Z plane and the plane containing both the pointer (e.g., pen stylus) axis and the Y axis. */
    tiltX: -1,
    /**Plane angle (in degrees, in the range of -90 to 90) between the X–Z plane and the plane containing both the pointer (e.g., pen stylus) axis and the X axis. */
    tiltY: -1,
    /**A unique identifier for the pointing device generating the PointerEvent. */
    persistentDeviceId: -1,
    /**A unique identifier for the pointer causing the event. */
    pointerId: -1,
    /**Indicates the device type that caused the event (mouse, pen, touch, etc.). */
    pointerType: "",
    /**Indicates if the pointer represents the primary pointer of this pointer type. */
    isPrimary: false,
}
const pointerdown_keys = Object.keys(pointerdown_data);
const pointerup_data = {
    /**time (in milliseconds) at which the event was created. */
    timeStamp: -1,
    /**Event type */
    type: "",
    /**The X coordinate of the pointer in viewport coordinates. */
    clientX: -1,
    /**The Y coordinate of the pointer in viewport coordinates. */
    clientY: -1,
    /**The X coordinate of the pointer relative to the position of the padding edge of the target node. */
    offsetX: -1,
    /**The Y coordinate of the pointer relative to the position of the padding edge of the target node. */
    offsetY: -1,
    /**The X coordinate of the pointer relative to the whole document. */
    pageX: -1,
    /**The X coordinate of the pointer relative to the whole document. */
    pageY: -1,
    /**The X coordinate of the pointer in screen coordinates. */
    screenX: -1,
    /**The X coordinate of the pointer in screen coordinates. */
    screenY: -1,
    /**The X coordinate of the pointer relative to the position of the last move event. */
    movementX: -1,
    /**The Y coordinate of the pointer relative to the position of the last move event. */
    movementY: -1,
    /**The width (magnitude on the X axis), in CSS pixels, of the contact geometry of the pointer. */
    width: -1,
    /**The height (magnitude on the Y axis), in CSS pixels, of the contact geometry of the pointer. */
    height: -1,
    /**Represents the angle between a transducer (a pointer or stylus) axis and the X-Y plane of a device screen. */
    altitudeAngle: -1,
    /**Represents the angle between the Y-Z plane and the plane containing both the transducer (a pointer or stylus) axis and the Y axis. */
    azimuthAngle: -1,
    /**The button number that was pressed or released (if applicable) when the mouse event was fired. */
    button: -1,
    /**The buttons being pressed (if any) when the event was fired. */
    buttons: -1,
    /**Normalized pressure of the pointer input in the range of 0 to 1, where 0 and 1 represent the minimum and maximum pressure the hardware is capable of detecting, respectively. */
    pressure: -1,
    /**Normalized tangential pressure of the pointer input (also known as barrel pressure or cylinder stress) in the range -1 to 1, where 0 is the neutral position of the control. */
    tangentialPressure: -1,
    /**Plane angle (in degrees, in the range of -90 to 90) between the Y–Z plane and the plane containing both the pointer (e.g., pen stylus) axis and the Y axis. */
    tiltX: -1,
    /**Plane angle (in degrees, in the range of -90 to 90) between the X–Z plane and the plane containing both the pointer (e.g., pen stylus) axis and the X axis. */
    tiltY: -1,
    /**A unique identifier for the pointing device generating the PointerEvent. */
    persistentDeviceId: -1,
    /**A unique identifier for the pointer causing the event. */
    pointerId: -1,
    /**Indicates the device type that caused the event (mouse, pen, touch, etc.). */
    pointerType: "",
    /**Indicates if the pointer represents the primary pointer of this pointer type. */
    isPrimary: false,
}
const pointerup_keys = Object.keys(pointerup_data);
function updatePointerEventData(event, pointer_data) {
    if (event.timeStamp < pointer_data.timeStamp) { return false; }
    pointer_data.isPrimary = event.isPrimary;
    pointer_data.altitudeAngle = event.altitudeAngle;
    pointer_data.azimuthAngle = event.azimuthAngle;
    pointer_data.button = event.button;
    pointer_data.buttons = event.buttons;
    pointer_data.clientX = event.clientX;
    pointer_data.clientY = event.clientY;
    pointer_data.height = event.height;
    pointer_data.movementX = event.movementX;
    pointer_data.movementY = event.movementY;
    pointer_data.offsetX = event.offsetX;
    pointer_data.offsetY = event.offsetY;
    pointer_data.pageX = event.pageX;
    pointer_data.pageY = event.pageY;
    pointer_data.persistentDeviceId = event.persistentDeviceId;
    pointer_data.pointerId = event.pointerId;
    pointer_data.pressure = event.pressure;
    pointer_data.screenX = event.screenX;
    pointer_data.screenY = event.screenY;
    pointer_data.tangentialPressure = event.tangentialPressure;
    pointer_data.tiltX = event.tiltX;
    pointer_data.tiltY = event.tiltY;
    pointer_data.timeStamp = event.timeStamp;
    pointer_data.width = event.width;
    pointer_data.pointerType = event.pointerType;
    pointer_data.type = event.type;
    return true;
}
async function pointermoveHandler(event) {
    // if (event.buttons <= 0) { return }
    if (updatePointerEventData(event, pointermove_data)) {
        requestAnimationFrame(update);
    }
}
async function pointerdownHandler(event) {
    if (updatePointerEventData(event, pointerdown_data)) {
        console.debug("pointerdown");
    }
}
async function pointerupHandler(event) {
    if (updatePointerEventData(event, pointerup_data)) {
        console.debug("pointerup");
    }
}
//#endregion