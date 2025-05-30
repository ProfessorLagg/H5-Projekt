const shapes = {};
const shapeIds = [];
async function loadShapes() {
    let req = fetch("data/shapes.json");
    for (key in shapes) { delete shapes[key]; }
    let rsp = await req;
    let new_shapes = await rsp.json();
    for (key in new_shapes) { shapes[key] = new_shapes[key]; }


    const new_shapeIds = Object
        .keys(shapes)
        .flatMap(x => Number(x))
        .filter(x => Number.isInteger(x))
        .filter(x => x >= 0);
    new_shapeIds.sort(function (a, b) { return a - b });
    shapeIds.length = new_shapeIds.length;
    for (let i = 0; i < new_shapeIds.length; i++) {
        shapeIds[i] = new_shapeIds[i];
    }
}
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
/**Used to save on GC while rendering shapes */
const temp_canvas = document.createElement("canvas");
function renderShape(shapeId, cellSize, blockimg) {
    TypeChecker.assertIsInteger(cellSize);
    if (!isValidShapeId(shapeId)) { throw Error(shapeId + " is not a valid shapeId"); }

    // TODO validate that blockimg is an Image
    const shape = shapes[shapeId];
    temp_canvas.width = cellSize * 5;
    temp_canvas.height = cellSize * 5;
    const ctx = temp_canvas.getContext("2d");
    for (let i = 0; i < shape.length; i++) {
        ctx.drawImage(blockimg, shape[i].c * cellSize, shape[i].r * cellSize, cellSize, cellSize)
    }
    return temp_canvas;
}