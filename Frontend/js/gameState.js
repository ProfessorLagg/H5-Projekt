// Game State
class GameState {
    prng = new sfc32(new Uint32Array(4));
    buffer = new ArrayBuffer(
        +81 // boardState
        + 6 // pieces
        + 4 // score
    );

    get score() { return this.scoreState[0]; }
    set score(v) {
        TypeChecker.assertIsInteger(v);
        this.scoreState[0] = v;
    }

    boardStateChangedCallback = () => {}
    pieceBufferChangedCallback = () => {}
    scoreChangedCallback = () => {}


    getCellState(cellId) {
        TypeChecker.assertIsInteger(cellId);
        if (cellId < 0 || cellId > 80) { throw Error("Invalid Cell ID: " + cellId); }
        return this.buffer[cellId] > 0;
    }

    generatePieces() {
        this.pieces[0] = this.prng.nextInt() % shapeIds.length;
        this.pieces[1] = this.prng.nextInt() % shapeIds.length;
        // TODO NO KILL PLZ
        this.pieces[2] = this.prng.nextInt() % shapeIds.length;
        this.pieceBufferChangedCallback();
    }
    restart() {
        this.prng.reset(sfc32.generateSeed());
        this.score = 0;
        this.boardState.fill(0)
        this.generatePieces();
    }
    constructor() {
        this.boardState = new Uint8Array(this.buffer.slice(0, 81));
        this.pieces = new Uint16Array(this.buffer.slice(81, 87));
        this.scoreState = new Uint32Array(this.buffer.slice(87));
        this.restart();
    }
}