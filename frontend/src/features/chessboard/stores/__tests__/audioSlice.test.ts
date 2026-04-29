import { StoreApi } from "zustand";

import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import { SpecialMoveType } from "@/lib/apiClient";
import { AnimationStep } from "../../lib/types";
import BoardPieces from "../../lib/boardPieces";

vi.mock("@/features/audio/audioPlayer");

describe("AudioSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("should not play audio if movedPieceIds is empty", async () => {
        const step: AnimationStep = {
            newPieces: new BoardPieces(),
            movedPieceIds: [],
        };

        await store.getState().playAudioForAnimationStep(step);

        expect(AudioPlayer.playAudio).not.toHaveBeenCalled();
    });

    it("should not play audio if muteAudio is true", async () => {
        store.setState({ muteAudio: true });
        const step: AnimationStep = {
            newPieces: new BoardPieces(),
            movedPieceIds: ["1"],
        };

        await store.getState().playAudioForAnimationStep(step);

        expect(AudioPlayer.playAudio).not.toHaveBeenCalled();
    });

    it("should play special move audio if step has specialMoveType", async () => {
        const step: AnimationStep = {
            newPieces: new BoardPieces(),
            movedPieceIds: ["1"],
            isCapture: true,
            specialType: SpecialMoveType.KNOOKLEAR_FUSION,
        };

        await store.getState().playAudioForAnimationStep(step);

        expect(AudioPlayer.playAudio).toHaveBeenCalledExactlyOnceWith(
            AudioType.EXPLOSION,
        );
    });

    it("should play capture audio if isCapture is true and no special move", async () => {
        const step: AnimationStep = {
            newPieces: new BoardPieces(),
            movedPieceIds: ["1"],
            isCapture: true,
        };

        await store.getState().playAudioForAnimationStep(step);

        expect(AudioPlayer.playAudio).toHaveBeenCalledExactlyOnceWith(
            AudioType.CAPTURE,
        );
    });

    it("should play normal move audio if no capture or special move", async () => {
        const step: AnimationStep = {
            newPieces: new BoardPieces(),
            movedPieceIds: ["1"],
        };

        await store.getState().playAudioForAnimationStep(step);

        expect(AudioPlayer.playAudio).toHaveBeenCalledExactlyOnceWith(
            AudioType.MOVE,
        );
    });

    it("should play both promotion and move audio when promotion", async () => {
        const step: AnimationStep = {
            newPieces: new BoardPieces(),
            movedPieceIds: ["1"],
            isPromotion: true,
        };

        await store.getState().playAudioForAnimationStep(step);

        expect(AudioPlayer.playAudio).toHaveBeenCalledWith(AudioType.PROMOTION);
        expect(AudioPlayer.playAudio).toHaveBeenCalledWith(AudioType.MOVE);
        expect(AudioPlayer.playAudio).toBeCalledTimes(2);
    });
});
