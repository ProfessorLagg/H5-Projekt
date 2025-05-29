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
function renderShape(shapeId, cellSize, blockimg) {
    const cell_size = Number(cellSize);
    if (cell_size < 1) { throw console.error("cellSize must be greater than 1, but was: " + cellSize); }
    if (!isValidShapeId(shapeId)) { return; }

    // TODO validate that blockimg is an Image
    const shape = shapes[shapeId];
    const result = document.createElement("canvas");
    result.width = cell_size * 5;
    result.height = cell_size * 5;
    const ctx = result.getContext("2d");
    for (let i = 0; i < shape.length; i++) {
        ctx.drawImage(blockimg, shape[i].x * cellSize, shape[i].y * cellSize, cellSize, cellSize)
    }
    return result;
}