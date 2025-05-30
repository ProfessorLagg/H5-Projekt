// Game State
class GameState {
    prng = new sfc32(new Uint32Array(4));
    buffer = new ArrayBuffer(
        + 81 // boardState
        + 6 // pieces
        + 4 // score
    );

    boardState = new Uint8Array(this.buffer.slice(0, 81));

    pieces = new Int16Array(this.buffer.slice(81, 87));
    selectedPieceId = -1;
    get selectedShapeId() {
        if (this.selectedPieceId < 0 || this.selectedPieceId > 2) { 
            console.error("No shape selected")
            return null;
         }
        const shapeId = this.pieces[this.selectedPieceId];
        assertIsValidShapeId(shapeId);
        return shapeId;
    }

    scoreState = new Uint32Array(this.buffer.slice(87));

    get score() { return this.scoreState[0]; }
    set score(v) {
        TypeChecker.assertIsInteger(v);
        this.scoreState[0] = v;
        this.scoreChangedCallback();
    }

    boardStateChangedCallback = () => { }
    pieceBufferChangedCallback = () => { }
    scoreChangedCallback = () => { }

    /**
     * Gets the filled state of a board cell by it's index
     * @param {Number} cellIndex 
     * @returns true if the cell is filled or false if it is empty
     */
    getCellState(cellIndex) {
        TypeChecker.assertIsIntegerInRange(cellIndex, 0, 80);
        return this.buffer[cellIndex] > 0;
    }

    generatePieces() {
        this.pieces[0] = this.prng.nextInt() % shapeIds.length;
        this.pieces[1] = this.prng.nextInt() % shapeIds.length;
        // TODO NO KILL PLZ
        this.pieces[2] = this.prng.nextInt() % shapeIds.length;
        this.pieceBufferChangedCallback();
    }
    selectPiece(pieceId) {
        TypeChecker.assertIsIntegerInRange(pieceId, 0, 2);
        assertIsValidShapeId(this.pieces[pieceId]);
        this.selectedPieceId = pieceId;
        console.debug("Selected piece " + this.selectedPieceId);
    }
    placeSelectedPiece(cellIndex) {
        throw Error("Not yet implemented");

        this.pieceBufferChangedCallback();
    }
    clearSelectedPiece() { 
        this.selectedPieceId = -1;
        console.debug("Selected piece " + this.selectedPieceId);
    }

    restart() {
        this.prng.reset(sfc32.generateSeed());
        this.scoreState[0] = 0;
        this.boardState.fill(0);
        this.pieces.fill(-1);

        this.generatePieces();

        /* Debug */
        this.boardState[40] == 1;
        this.boardStateChangedCallback();
    }
    constructor() {
        // this.boardState = new Uint8Array(this.buffer.slice(0, 81));
        // this.pieces = new Int16Array(this.buffer.slice(81, 87));
        // this.scoreState = new Uint32Array(this.buffer.slice(87));
        // this.restart();
    }
}