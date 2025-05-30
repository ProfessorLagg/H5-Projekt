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
function distanceSquared2D(x0, y0, x1, y1) {
    const a = Math.max(x0, x1) - Math.min(x0, x1);
    const b = Math.max(y0, y1) - Math.min(y0, y1);
    const a2 = a * a;
    const b2 = b * b;
    return a2 + b2;
}
function distance2D(x0, y0, x1, y1) {
    return Math.sqrt(distanceSquared2D(x0, y0, x1, y1));
}