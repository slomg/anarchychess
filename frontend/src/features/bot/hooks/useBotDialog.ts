import { useRef } from "react";

import { randomItem, seededRandomItem } from "@/lib/utils/randomUtils";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { PlayerType } from "@/features/liveGame/lib/types";
import { GameColor, GameResult } from "@/lib/apiClient";
import { Move } from "@/features/chessboard/lib/types";

export interface DialogContext {
    move: Move;
    prevPieces: BoardPieces;
    playerType: PlayerType;
    plyNumber: number;
    evalForBot: number | null;
    prevEvalForBot: number | null;
}

export interface ReactionDialog {
    condition: (ctx: DialogContext) => boolean;
    lines: string[];
}

export interface BotDialogOptions {
    reactionDialog: ReactionDialog[];
    generalDialog: string[];
    startDialog: string[];
    botWinDialog: string[];
    botLoseDialog: string[];
}

export default function useBotDialog({
    reactionDialog,
    generalDialog,
    startDialog,
    botWinDialog,
    botLoseDialog,
}: BotDialogOptions): {
    getDialogForMove(ctx: DialogContext): string | null;
    getDialogForGameStart(gameToken: string): string | null;
    getDialogForGameEnd(
        result: GameResult,
        botColor: GameColor,
        gameToken: string,
    ): string | null;
} {
    const usedReactionLineIdxesRef = useRef<Set<number>>(new Set());
    const usedGeneralLinesIdxesRef = useRef<Set<number>>(new Set());

    const nextGeneralDialogPlyRef = useRef<number | null>(null);
    const lastLineAtRef = useRef<number | null>(null);

    function getDialogForMove(ctx: DialogContext): string | null {
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

        return null;
    }

    function getDialogForGameStart(gameToken: string): string | null {
        return seededRandomItem(startDialog, gameToken);
    }

    function getDialogForGameEnd(
        result: GameResult,
        botColor: GameColor,
        gameToken: string,
    ): string | null {
        const botWinResult =
            botColor === GameColor.WHITE
                ? GameResult.WHITE_WIN
                : GameResult.BLACK_WIN;
        const lines = result === botWinResult ? botWinDialog : botLoseDialog;
        return seededRandomItem(lines, gameToken);
    }

    function pickReactionLine(ctx: DialogContext): string | null {
        const availableReactionIndexes: number[] = [];
        for (let i = 0; i < reactionDialog.length; i++) {
            const line = reactionDialog[i];
            if (
                line.condition(ctx) &&
                !usedReactionLineIdxesRef.current.has(i)
            ) {
                availableReactionIndexes.push(i);
            }
        }

        const randomIdx = randomItem(availableReactionIndexes);
        if (randomIdx === null) {
            return null;
        }
        usedReactionLineIdxesRef.current.add(randomIdx);

        const lines = reactionDialog[randomIdx].lines;
        return randomItem(lines);
    }

    function pickGeneralLine(plyNumber: number): string | null {
        if (nextGeneralDialogPlyRef.current === null) {
            nextGeneralDialogPlyRef.current =
                plyNumber + getNextGeneralLineInterval(plyNumber);
            return null;
        }

        if (plyNumber < nextGeneralDialogPlyRef.current) {
            return null;
        }

        const availableGeneralIndexes: number[] = [];
        for (let i = 0; i < generalDialog.length; i++) {
            if (!usedGeneralLinesIdxesRef.current.has(i)) {
                availableGeneralIndexes.push(i);
            }
        }

        const randomIdx = randomItem(availableGeneralIndexes);
        if (randomIdx === null) {
            return null;
        }

        usedGeneralLinesIdxesRef.current.add(randomIdx);
        nextGeneralDialogPlyRef.current =
            plyNumber + getNextGeneralLineInterval(plyNumber);

        return generalDialog[randomIdx];
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

    return {
        getDialogForMove,
        getDialogForGameStart,
        getDialogForGameEnd,
    };
}
