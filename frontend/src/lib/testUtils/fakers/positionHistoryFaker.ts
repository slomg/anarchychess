import PositionHistory from "@/features/chessboard/lib/positionHistory";
import { PositionProps } from "@/features/chessboard/lib/position";
import { createFakeBoardPieces } from "./chessboardFakers";
import { createFakePositionProps } from "./positionPropsFaker";
import BoardPieces from "@/features/chessboard/lib/boardPieces";

export function createNFakePositionHistory(amount: number): PositionHistory {
    const positionHistory = new PositionHistory(createFakeBoardPieces());
    for (let i = 0; i < amount; i++) {
        positionHistory.addNextPosition(createFakePositionProps());
    }

    return positionHistory;
}

export function createFakePositionHistory({
    rootPieces,
    pos,
}: {
    rootPieces?: BoardPieces;
    pos: PositionProps[];
}): PositionHistory {
    rootPieces ??= createFakeBoardPieces();
    const positionHistory = new PositionHistory(rootPieces);
    for (const props of pos) {
        positionHistory.addNextPosition(props);
    }
    return positionHistory;
}
