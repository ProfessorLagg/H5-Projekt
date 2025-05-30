const shapes = {};
const shapeIds = [];
const shapeOffsetBounds = {}
function isValidShapeId(shapeId) {
    if (typeof shapeId !== "number") {
        console.error("shapeId must be a number");
        return false;
    }

    if (!Number.isInteger(shapeId)) {
        console.error("shapeId must be an integer");
        return false;
    }

    if (shapeIds.indexOf(shapeId) < 0) {
        console.error("no shape has the shapeId " + shapeId);
        return false;
    }

    return true;
}
function assertIsValidShapeId(shapeId) {
    if (typeof shapeId !== "number") {
        throw Error("shapeId must be a number");
    }

    if (!Number.isInteger(shapeId)) {
        throw Error("shapeId must be an integer");
    }

    if (shapeIds.indexOf(shapeId) < 0) {
        throw Error("no shape has the shapeId " + shapeId);
    }
}
/**
 * Finds the min/max col/row of a shape
 * @param {Number} shapeId 
 * @returns An Object with the keys: min_col, min_row, max_col, max_row, col_center_offset, row_center_offset
 */
function getShapeOffsetBounds(shapeId) {
    assertIsValidShapeId(shapeId);
    if (shapeOffsetBounds[shapeId] === undefined) {
        const shape = shapes[shapeId];
        shapeOffsetBounds[shapeId] = {
            min_col: Number.MAX_SAFE_INTEGER,
            min_row: Number.MAX_SAFE_INTEGER,
            max_col: Number.MIN_SAFE_INTEGER,
            max_row: Number.MIN_SAFE_INTEGER,
            col_center_offset: 0,
            row_center_offset: 0,
        }
        for (let i = 0; i < shape.length; i++) {
            shapeOffsetBounds[shapeId].min_col = Math.min(shapeOffsetBounds[shapeId].min_col, shape[i].c);
            shapeOffsetBounds[shapeId].min_row = Math.min(shapeOffsetBounds[shapeId].min_row, shape[i].r);
            shapeOffsetBounds[shapeId].max_col = Math.max(shapeOffsetBounds[shapeId].max_col, shape[i].c);
            shapeOffsetBounds[shapeId].max_row = Math.max(shapeOffsetBounds[shapeId].max_row, shape[i].r);
        }
        shapeOffsetBounds[shapeId].col_center_offset = Math.floor((5 - shapeOffsetBounds[shapeId].max_col) / 2)
        shapeOffsetBounds[shapeId].row_center_offset = Math.floor((5 - shapeOffsetBounds[shapeId].max_row) / 2)
    }
    return shapeOffsetBounds[shapeId];
}

/**Used to save on GC while rendering shapes */
const temp_canvas = document.createElement("canvas");
function renderShape(shapeId, cellSize, blockimg, centerX = false, centerY = false) {
    TypeChecker.assertIsInteger(cellSize);
    if (!isValidShapeId(shapeId)) { throw Error(shapeId + " is not a valid shapeId"); }

    // TODO allow offsetting to center of mass for x/y individually

    const shape = shapes[shapeId];
    const bounds = getShapeOffsetBounds(shapeId);

    // Calculate centering offsets
    const add_col = bounds.col_center_offset * Number(centerX);
    const add_row = bounds.row_center_offset * Number(centerY);

    temp_canvas.width = cellSize * 5;
    temp_canvas.height = cellSize * 5;
    const ctx = temp_canvas.getContext("2d");
    for (let i = 0; i < shape.length; i++) {
        ctx.drawImage(
            blockimg,
            (shape[i].c + add_col) * cellSize,
            (shape[i].r + add_row) * cellSize,
            cellSize,
            cellSize
        );
    }
    return temp_canvas;
}

async function loadShapes() {
    // Update shapes
    const req = fetch("data/shapes.json");
    for (key in shapes) { delete shapes[key]; }
    const rsp = await req;
    const new_shapes = await rsp.json();
    for (key in new_shapes) { shapes[key] = new_shapes[key]; }

    // Update shapeIds
    const new_shapeIds = Object
        .keys(shapes)
        .flatMap(x => Number(x))
        .filter(x => Number.isInteger(x))
        .filter(x => x >= 0);
    new_shapeIds.sort(function (a, b) { return a - b });
    shapeIds.length = new_shapeIds.length;
    for (let i = 0; i < new_shapeIds.length; i++) { shapeIds[i] = new_shapeIds[i]; }

    // Update shapeCenters
    for (key in shapeOffsetBounds) { delete shapeOffsetBounds[key]; }
    for (let i = 0; i < shapeIds.length; i++) {
        _ = getShapeOffsetBounds(shapeIds[i]);
    }
}