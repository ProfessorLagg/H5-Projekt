const game_wrap = document.getElementById('game-wrap');
const layer0 = document.getElementById('layer0');
const layer1 = document.getElementById('layer1');
const layer2 = document.getElementById('layer2');
const block_img = document.getElementById('block-img');


const cell_size = 100;
const border_size = 5;
const canvas_size = (cell_size * 9) + (border_size * 11);
const group_size = canvas_size / 3;

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

async function init() {
    layer0.width = canvas_size;
    layer0.height = canvas_size;
    layer1.width = canvas_size;
    layer1.height = canvas_size;
    layer2.width = canvas_size;
    layer2.height = canvas_size;

    draw_layer0();
    draw_layer1();
    draw_layer2();
}

function draw_layer0() {
    const ctx = layer0.getContext("2d");
    ctx.fillStyle = "rgba(100,45,0,100%)";
    ctx.fillRect(0, 0, canvas_size, canvas_size);

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

function draw_layer1() {
    const ctx = layer1.getContext("2d");
    ctx.clearRect(0, 0, canvas_size, canvas_size);
    ctx.fillStyle = "rgba(255,0,255,50%";
    const cells = getCellDrawBounds();
    for (let i = 0; i < cells.length; i++) {
        const cell = cells[i];
        if (cellFilled(i)) {
            ctx.drawImage(block_img, cell.x, cell.y, cell.w, cell.h);
        }
    }
}

function draw_layer2() {
    let cell_border_img = document.createElement("canvas");
    cell_border_img.width = canvas_size / 9;
    cell_border_img.height = canvas_size / 9;
    const cell_border_ctx = cell_border_img.getContext("2d");

    cell_border_ctx.strokeStyle = "white";
    cell_border_ctx.lineWidth = border_size;
    cell_border_ctx.shadowBlur = cell_size / 5;
    cell_border_ctx.shadowColor = "black";
    cell_border_ctx.strokeRect(
        0,
        0,
        canvas_size / 9,
        canvas_size / 9
    );

    const ctx = layer2.getContext("2d");

    ctx.strokeStyle = "white";
    ctx.lineWidth = border_size;
    ctx.strokeStyle = "white";
    ctx.strokeRect(0, 0, canvas_size, canvas_size); // Draw outer border

    const innerCanvas = {
        left: border_size / 2,
        top: border_size / 2,
        right: canvas_size - (border_size / 2),
        bot: canvas_size - (border_size / 2),
        width: (canvas_size - (border_size / 2)) - (border_size / 2),
        height: (canvas_size - (border_size / 2)) - (border_size / 2),
    }
    const cells = getCellDrawBounds();
    for (let i = 0; i < cells.length; i++) {
        const C = cells[i];
        const left = rangeMapNumber(C.x, 0, canvas_size, innerCanvas.left, innerCanvas.right);
        const top = rangeMapNumber(C.y, 0, canvas_size, innerCanvas.top, innerCanvas.bot);
        const right = rangeMapNumber(C.x + C.w, 0, canvas_size, innerCanvas.left, innerCanvas.right);
        const bot = rangeMapNumber(C.y + C.h, 0, canvas_size, innerCanvas.top, innerCanvas.bot);
        const width = right - left;
        const height = bot - top;
        ctx.drawImage(cell_border_img, left, top, width, height);
    }

}

function getCellDrawBounds() {
    let result = [];
    result.length = 81;

    const s = canvas_size / 9;
    let i = 0;

    for (let r = 0; r < 9; r++) {
        for (let c = 0; c < 9; c++) {
            result[i] = {
                x: c * s,
                y: r * s,
                w: s,
                h: s
            };
            i++;
        }
    }
    return result;
}

function cellFilled(cellId) {
    // TODO make this not a debug version
    return Math.random() >= 0.33;
}