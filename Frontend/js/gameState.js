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

    getClearableSections() {
        const result = [];
        for (let I = 0; I < 9; I++) {
            const grp_indexes = getGroupCellIndexes(I);
            const row_indexes = getRowCellIndexes(I);
            const col_indexes = getColumnCellIndexes(I);

            let grp_clearable = true;
            let row_clearable = true;
            let col_clearable = true;
            for (let i = 0; i < 9; i++) {
                const grp_info = grp_indexes[i];
                const row_info = row_indexes[i];
                const col_info = col_indexes[i];
                grp_clearable = grp_clearable && (this.boardState[grp_info.idx] > 0);
                row_clearable = row_clearable && (this.boardState[row_info.idx] > 0);
                col_clearable = col_clearable && (this.boardState[col_info.idx] > 0);
            }
            if (grp_clearable) { result.push(grp_indexes) }
            if (row_clearable) { result.push(row_indexes) }
            if (col_clearable) { result.push(col_indexes) }
        }
        return result;
    }

    clearSections() {
        const clearable_sections = this.getClearableSections();
        for (let I = 0; I < clearable_sections.length; I++) {
            const section = clearable_sections[I];
            for (let i = 0; i < section.length; i++) {
                this.boardState[section[i].idx] = 0;
                this.score += 1;
            }
            this.boardStateChangedCallback()
        }
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
    tryPlaceSelectedPiece(cell_index1D) {
        console.debug("GameState.tryPlaceSelectedPiece", "cellIndex:", cell_index1D);
        const cell_index2D = indexTo2D(cell_index1D, true);

        // TODO cache this somehow
        const shapeId = this.selectedShapeId;
        const shape = getShape(shapeId);
        const cellIndexes = new Array();
        for (let i = 0; i < shape.length; i++) {
            const row = shape[i].r + cell_index2D.row - Math.round(shapeOffsetBounds[shapeId].height / 2);
            if (row < 0 || row > 8) { return false }
            const col = shape[i].c + cell_index2D.col - Math.round(shapeOffsetBounds[shapeId].width / 2);
            if (col < 0 || col > 8) { return false }
            const idx = indexTo1D(row, col, true);
            if (this.boardState[idx] > 0) { return false }
            cellIndexes.push(idx);
        }

        for (let i = 0; i < cellIndexes.length; i++) {
            this.boardState[cellIndexes[i]] = 1;
            this.score += 1;
        }
        this.boardStateChangedCallback();

        this.pieces[this.selectedPieceId] = -1;
        this.clearSelectedPiece();

        // clear rows/cols/grps
        this.clearSections();

        // regen piece buffer if needed
        if (this.pieces[0] + this.pieces[1] + this.pieces[2] === -3) {
            this.generatePieces();
        }


        return true;
    }
    hasSelectedPiece() {
        return this.selectedPieceId >= 0 && this.selectedPieceId <= 2;
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