const gameScoreElem = document.getElementById('game-score');
function setScore(number) {
    console.assert(isNumber(number));
    gameScoreElem.value = number;
}
function getScore() {
    const scoreString = gameScoreElem.value;
    const scoreParsed = parseInt(scoreString);
    if (scoreParsed == NaN) {
        resetScore();
        return 0;
    }
    return scoreParsed;
}
function addScore(number) {
    console.assert(isNumber(number))
    const old_score = getScore();
    const new_score = old_score + number;
    setScore(new_score);
}
function testScore() {
    const test_val = 2;
    let passed = true;
    setScore(test_val);
    passed = passed && expectEqual(test_val, getScore());
    addScore(test_val);
    passed = passed && expectEqual(test_val + test_val, getScore());
    return passed;
}

const gameBoardElem = document.getElementById('game-board');
const boardCellElems = new Array(9).fill(0).map(() => new Array(9).fill(0));;

function newBoardCell(row, col) {
    // TODO should be web component
    console.assert(isNumber(row));
    console.assert(row >= 0);
    console.assert(row < 9);
    console.assert(isNumber(col));
    console.assert(col >= 0);
    console.assert(col < 9);

    const cellId = 9 * row + col;
    const cell_color_class = cellId % 2 === 0 ? 'board-cell-light' : 'board-cell-dark';
    let elem = document.createElement('div');
    elem.classList.add('board-cell');
    elem.classList.add(cell_color_class);
    elem.setAttribute('row', row);
    elem.setAttribute('col', row);
    elem.setAttribute('id', cellId);
    elem.setAttribute('filled', 0);
    return elem;
}
function initGameBoard() {
    Array.from(gameBoardElem.children).forEach(e => e.remove())
    for (let r = 0; r < 9; r++) {
        for (let c = 0; c < 9; c++) {
            const cellElem = newBoardCell(r, c);
            gameBoardElem.appendChild(cellElem);
            boardCellElems[r][c] = cellElem;
        }
    }
}

function init() {
    initGameBoard();
}