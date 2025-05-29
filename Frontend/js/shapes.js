const shapes = {};
async function loadShapes() {
    let req = fetch("data/shapes.json");
    for (key in shapes) { delete shapes[key]; }
    let rsp = await req;
    let new_shapes = await rsp.json();
    for (key in new_shapes) { shapes[key] = new_shapes[key]; }
}