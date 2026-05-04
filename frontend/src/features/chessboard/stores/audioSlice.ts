import { StateCreator } from "zustand";

import type { ChessboardStore } from "./chessboardStore";
import { SpecialMoveType } from "@/lib/apiClient";
import { AnimationStep } from "../lib/types";
import AudioPlayer, { AudioType } from "../../audio/audioPlayer";

export interface AudioSliceProps {
    muteAudio?: boolean;
}

export interface AudioSlice {
    muteAudio: boolean;
    playAudioForAnimationStep(step: AnimationStep): Promise<void>;
}

const SPECIAL_MOVE_AUDIO_MAP: Partial<Record<SpecialMoveType, AudioType>> = {
    [SpecialMoveType.KNOOKLEAR_FUSION]: AudioType.EXPLOSION,
    [SpecialMoveType.KINGSIDE_CASTLE]: AudioType.CASTLE,
    [SpecialMoveType.QUEENSIDE_CASTLE]: AudioType.CASTLE,
    [SpecialMoveType.VERTICAL_CASTLE]: AudioType.CASTLE,
    [SpecialMoveType.IL_VATICANO]: AudioType.CASTLE,
    [SpecialMoveType.THROW]: AudioType.EXPLOSION,
    [SpecialMoveType.QUEENTUM_TUNNEL]: AudioType.QUEENTUM_TUNNEL,
};

export function createAudioSlice(
    initState: AudioSliceProps,
): StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    AudioSlice
> {
    return (_, get) => ({
        muteAudio: initState.muteAudio ?? false,
        cachedAudios: new Map(),

        async playAudioForAnimationStep(step) {
            const { muteAudio } = get();
            if (muteAudio || step.mute) return;

            const specialMoveAudio = step.specialType
                ? SPECIAL_MOVE_AUDIO_MAP[step.specialType]
                : null;
            if (specialMoveAudio) {
                await AudioPlayer.playAudio(specialMoveAudio);
                return;
            }

            if (step.isPromotion) {
                await AudioPlayer.playAudio(AudioType.PROMOTION);
            }

            if (step.isCapture) {
                await AudioPlayer.playAudio(AudioType.CAPTURE);
            } else {
                await AudioPlayer.playAudio(AudioType.MOVE);
            }
        },
    });
}
