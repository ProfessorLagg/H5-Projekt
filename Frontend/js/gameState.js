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
    selectionChangedCallback = () => { }

    /**
     * Gets the filled state of a board cell by it's index
     * @param {Number} cellIndex 
     * @returns true if the cell is filled or false if it is empty
     */
    getCellState(cellIndex) {
        TypeChecker.assertIsIntegerInRange(cellIndex, 0, 80);
        return this.boardState[cellIndex] > 0;
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
        this.selectionChangedCallback();
    }
    clearSelectedPiece() {
        this.selectedPieceId = -1;
        console.debug("Selected piece " + this.selectedPieceId);
        this.selectionChangedCallback();
    }
    tryPlaceSelectedPiece(cellIndex) {
        console.debug("GameState.tryPlaceSelectedPiece", "cellIndex:", cellIndex);
        if (!TypeChecker.isIntegerInRange(cellIndex, 0, 80)) { return false }
        const cell_index2D = indexTo2D(cellIndex, true);
        // TODO cache this somehow
        const shapeId = this.selectedShapeId;
        const shape = getShape(shapeId);
        const cellIndexes = new Uint8Array(shape.length);
        for (let i = 0; i < shape.length; i++) {
            const row = cell_index2D.row + shape[i].r;
            if (row < 0 || row > 8) { return false }
            const col = cell_index2D.col + shape[i].c;
            if (col < 0 || col > 8) { return false }
            const idx = indexTo1D(row, col, true);
            if (this.boardState[idx] === 1) { return false }
            cellIndexes[i] = idx;
        }

        for (let i = 0; i < cellIndexes.length; i++) {
            this.boardState[cellIndexes[i]] = 1;
            this.score += 1;
        }

        // TODO clear rows/cols/grps

        // TODO regen piece buffer if needed

        this.pieces[this.selectedPieceId] = -1;
        this.clearSelectedPiece();
        this.boardStateChangedCallback();
        return true;
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
    }
}