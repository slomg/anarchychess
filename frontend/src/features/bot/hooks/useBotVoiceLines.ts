import { useRef } from "react";

import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { PlayerType } from "@/features/liveGame/lib/types";
import { Move } from "@/features/chessboard/lib/types";

export interface VoiceLineContext {
    move: Move;
    prevPieces: BoardPieces;
    playerType: PlayerType;
    plyNumber: number;
    evalForBot: number | null;
    prevEvalForBot: number | null;
}

export interface ReactionVoiceLine {
    condition: (ctx: VoiceLineContext) => boolean;
    lines: string[];
}

export interface LoreVoiceLine {
    onPly: number;
    line: string;
}

export default function useBotVoiceLines(
    reactionVoiceLines: ReactionVoiceLine[],
    loreVoiceLines: LoreVoiceLine[],
    generalVoiceLines: string[],
): (ctx: VoiceLineContext) => string | null {
    const usedReactionLineIdxesRef = useRef<Set<number>>(new Set());
    const usedGeneralLinesIdxesRef = useRef<Set<number>>(new Set());

    const nextGeneralVoiceLinePlyRef = useRef<number | null>(null);
    const lastLoreLineForPlyNumberRef = useRef<number>(0);
    const lastLineAtRef = useRef<number | null>(null);

    function getVoiceLine(ctx: VoiceLineContext): string | null {
        if (
            lastLineAtRef.current !== null &&
            ctx.plyNumber - lastLineAtRef.current < 2
        ) {
            return null;
        }

        let line = pickReactionLine(ctx);
        if (line) {
            lastLineAtRef.current = ctx.plyNumber;
            return line;
        }

        line = pickGeneralLine(ctx.plyNumber);
        if (line) {
            lastLineAtRef.current = ctx.plyNumber;
            return line;
        }

        line = pickLoreLine(ctx.plyNumber);
        if (line) {
            lastLineAtRef.current = ctx.plyNumber;
            return line;
        }

        return null;
    }

    function pickReactionLine(ctx: VoiceLineContext): string | null {
        const availableReactionIndexes: number[] = [];
        for (let i = 0; i < reactionVoiceLines.length; i++) {
            const line = reactionVoiceLines[i];
            if (
                line.condition(ctx) &&
                !usedReactionLineIdxesRef.current.has(i)
            ) {
                availableReactionIndexes.push(i);
            }
        }
        if (availableReactionIndexes.length === 0) {
            return null;
        }

        const randomIdx =
            availableReactionIndexes[
                Math.floor(Math.random() * availableReactionIndexes.length)
            ];
        usedReactionLineIdxesRef.current.add(randomIdx);

        const lines = reactionVoiceLines[randomIdx].lines;
        return lines[Math.floor(Math.random() * lines.length)];
    }

    function pickGeneralLine(plyNumber: number): string | null {
        if (nextGeneralVoiceLinePlyRef.current === null) {
            nextGeneralVoiceLinePlyRef.current =
                plyNumber + getNextGeneralLineInterval(plyNumber);
            return null;
        }

        if (plyNumber < nextGeneralVoiceLinePlyRef.current) {
            return null;
        }

        const availableGeneralIndexes: number[] = [];
        for (let i = 0; i < generalVoiceLines.length; i++) {
            if (!usedGeneralLinesIdxesRef.current.has(i)) {
                availableGeneralIndexes.push(i);
            }
        }
        if (availableGeneralIndexes.length === 0) {
            return null;
        }

        nextGeneralVoiceLinePlyRef.current =
            plyNumber + getNextGeneralLineInterval(plyNumber);

        const randomIdx =
            availableGeneralIndexes[
                // eslint-disable-next-line react-hooks/purity -- why tf is it complaining about that here
                Math.floor(Math.random() * availableGeneralIndexes.length)
            ];
        usedGeneralLinesIdxesRef.current.add(randomIdx);
        return generalVoiceLines[randomIdx];
    }

    function pickLoreLine(plyNumber: number): string | null {
        const availableLoreIndexes: number[] = [];
        for (let i = 0; i < loreVoiceLines.length; i++) {
            const line = loreVoiceLines[i];
            if (
                line.onPly > lastLoreLineForPlyNumberRef.current &&
                line.onPly <= plyNumber
            ) {
                availableLoreIndexes.push(i);
            }
        }
        if (availableLoreIndexes.length === 0) {
            return null;
        }

        lastLoreLineForPlyNumberRef.current = plyNumber;
        const randomIdx =
            availableLoreIndexes[
                Math.floor(Math.random() * availableLoreIndexes.length)
            ];
        return loreVoiceLines[randomIdx].line;
    }

    function getNextGeneralLineInterval(plyNumber: number) {
        const startMin = 4;
        const startMax = 8;
        const endMin = 14;
        const endMax = 30;

        const totalPliesEstimate = 120;

        const progress = Math.min(plyNumber / totalPliesEstimate, 1);
        const min = startMin + progress * (endMin - startMin);
        const max = startMax + progress * (endMax - startMax);

        return Math.floor(Math.random() * (max - min + 1)) + min;
    }

    return getVoiceLine;
}
