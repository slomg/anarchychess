import { useEffect, useEffectEvent, useRef, useState } from "react";
import { CSSTransition } from "react-transition-group";
import { StoreApi } from "zustand";

import useBotVoiceLines, {
    LoreVoiceLine,
    ReactionVoiceLine,
} from "../hooks/useBotVoiceLines";
import {
    GameColor,
    MoveSnapshot,
    PieceType,
    SpecialMoveType,
} from "@/lib/apiClient";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import useLiveChessStore from "@/features/liveGame/hooks/useLiveChessStore";
import ProfilePicture from "@/features/profile/components/ProfilePicture";
import { decodeMovePath } from "@/features/liveGame/lib/moveDecoder";
import { PlayerType } from "@/features/liveGame/lib/types";
import { useBotEvent } from "../hooks/useBotHub";
import Card from "@/components/ui/Card";

const reactionVoiceLines: ReactionVoiceLine[] = [
    {
        condition: ({ move, playerType }) =>
            move.specialType === SpecialMoveType.EN_PASSANT &&
            playerType === PlayerType.Human,
        lines: [
            "omgfom gomgmo EN FUCKING CROISSANT?",
            "One does not simply en passant me.",
            "wtf did you just do? THIS MOVE IS CHEATING!",
            "Since when is this allowed??",
            "I bet you couldn’t play any other move",
            "holy hell",
        ],
    },
    {
        condition: ({ evalForBot, prevEvalForBot }) =>
            evalForBot !== null &&
            prevEvalForBot !== null &&
            evalForBot - prevEvalForBot > 300 &&
            evalForBot < 99_000,
        lines: [
            "oi m8 that move was a wee bit shit, innit?",
            "Hm yes that move is very very poggies indeed.",
            "Maybe think before playing next time.",
            "Thanks! *dry humps your leg*",
            "Gavin from 3rd grade, he is the strongest player. Maybe you should learn from him.",
            "I'm just gonna be real honest with you, that was not a good move",
        ],
    },
    {
        condition: ({ evalForBot }) =>
            evalForBot !== null && evalForBot >= 100_000,
        lines: ["omae wa mou shindeiru"],
    },
    {
        condition: ({ move }) =>
            move.specialType === SpecialMoveType.RADIOACTIVE_BETA_DECAY,
        lines: [
            "For your safety, I would recommend getting away from that pawn.",
        ],
    },
    {
        condition: ({ move }) =>
            move.specialType === SpecialMoveType.KNOOKLEAR_FUSION,
        lines: [
            "If we can't get nuclear fusion power plants working, maybe we should try knooklear fusion.",
        ],
    },
    {
        condition: ({ move }) =>
            move.specialType === SpecialMoveType.OMNIPOTENT_PAWN_SPAWN,
        lines: ["Where did that come from?"],
    },
    {
        condition: ({ move, prevPieces }) => {
            if (
                prevPieces.getByPosition(move.from)?.type !== PieceType.BISHOP
            ) {
                return false;
            }

            for (const capture of move.captures) {
                if (
                    prevPieces.getByPosition(capture)?.type ===
                    PieceType.UNDERAGE_PAWN
                ) {
                    return true;
                }
            }
            return false;
        },
        lines: ["mmm yummy children"],
    },
];

const loreVoiceLines: LoreVoiceLine[] = [];

const generalVoiceLines: string[] = [
    "Did anybody notice Alexandra doesn't have any visible tattoos? It's unusual for an American woman",
    "Carlsen is unquestionably one of the players in chess history",
    "I don't get it. Mona Lisa is ugly as fuck.",
    "that got ham cheese guy really is something",
    "Ever heard of the Home by Phillip Phillips opening?",
    "Filth pig go play dominoes because you have no idea about the tu art of chess. You are rubbish and an insult to the art. The filth of the art. Useless filth,",
    "I literally do not care.",
    "Do you take bribes?",
    "I do wonder why the bishop is so obsessed with the underage pawns sometimes.",
    "chess.c*m could never",
    'Am I the only one that pronounces "anarchy chess" like "anal bead cheese"?',
    "Anyways, what's your favorite dinosaur?",
    "Why does the bishop have that slit?",
    "This position smells like burnt toast.",
    "I FUCKING LOVE PUSHING PAWNS FOR NO REASON!!! RWAAAA GIVE ME ALL THE SPACE!@@!!!",
    "Why does the horsey move like that? Is it afraid of straight lines? Is it gay?",
    "Is this theory?",
    "I AM A HALF HORSEEE, HALF MAN!!! HOW COULD SHE EVER UNDERSTANd",
    "I'm gonna add this game to my tower of random objects.",
    "gump",
];

const BotVoiceLines = ({
    botColor,
    chessboardStore,
}: {
    botColor: GameColor;
    chessboardStore: StoreApi<ChessboardStore>;
}) => {
    const [voiceLine, setVoiceLine] = useState<string | null>(null);

    const voiceLineHistoryRef = useRef<Map<number, string | null>>(new Map());
    const prevEvalForBotRef = useRef<number | null>(null);

    const { gameToken, botPlayer } = useLiveChessStore((x) => ({
        gameToken: x.gameToken,
        botPlayer: x.getPlayerByColor(botColor),
    }));

    const getVoiceLine = useBotVoiceLines(
        reactionVoiceLines,
        loreVoiceLines,
        generalVoiceLines,
    );

    function handleMove(
        move: MoveSnapshot,
        plyNumber: number,
        playerType: PlayerType,
        evalForBot: number | null,
        didMoveEndGame: boolean,
    ) {
        if (didMoveEndGame) {
            return;
        }

        const { positionHistory, boardDimensions } = chessboardStore.getState();
        const prevPosition = positionHistory.getPositionWithPly(plyNumber - 1);
        if (!prevPosition) {
            return;
        }
        const decodedMove = decodeMovePath(move.path, boardDimensions.width);
        const newVoiceLine = getVoiceLine({
            move: decodedMove,
            prevPieces: prevPosition.pieces,
            playerType,
            plyNumber,
            evalForBot,
            prevEvalForBot: prevEvalForBotRef.current,
        });

        if (newVoiceLine) {
            setVoiceLine(newVoiceLine);
            voiceLineHistoryRef.current.set(plyNumber, newVoiceLine);
        } else {
            const prevVoiceLine =
                voiceLineHistoryRef.current.get(plyNumber - 1) ?? null;
            voiceLineHistoryRef.current.set(plyNumber, prevVoiceLine);
        }

        if (evalForBot !== null) {
            prevEvalForBotRef.current = evalForBot;
        }
    }

    useBotEvent(
        gameToken,
        "BotMadeMoveAsync",
        (move, plyNumber, _, evalForBot, didMoveEndGame) =>
            handleMove(
                move,
                plyNumber,
                PlayerType.Bot,
                evalForBot,
                didMoveEndGame,
            ),
    );
    useBotEvent(
        gameToken,
        "PlayerMadeMoveAsync",
        (move, plyNumber, didMoveEndGame) =>
            handleMove(move, plyNumber, PlayerType.Human, null, didMoveEndGame),
    );

    const voiceBubbleRef = useRef<HTMLDivElement>(null);
    const [isVisible, setIsVisible] = useState<boolean>(false);
    const [displayVoiceLine, setDisplayVoiceLine] = useState<string | null>(
        null,
    );
    const fadeEvent = useEffectEvent(() => {
        if (!voiceLine) {
            setIsVisible(false);
            return;
        }

        setIsVisible(false);
        const timeout = setTimeout(() => {
            setIsVisible(true);
            setDisplayVoiceLine(voiceLine);
        }, 300);
        return timeout;
    });
    useEffect(() => {
        const timeout = fadeEvent();
        return () => clearTimeout(timeout);
    }, [voiceLine]);

    const viewingPlyNumber = useChessboardStore(
        (x) => x.positionHistory.viewingPosition?.ply,
    );
    useEffect(() => {
        if (viewingPlyNumber == null) {
            return;
        }

        const storedVoiceLine =
            voiceLineHistoryRef.current.get(viewingPlyNumber);
        if (storedVoiceLine !== undefined) {
            setVoiceLine(storedVoiceLine);
        }
    }, [viewingPlyNumber]);

    return (
        <Card className="flex-1 flex-row gap-5">
            <ProfilePicture
                userId={botPlayer.userId}
                minSize={120}
                size={120}
                data-testid="botVoiceLineProfilePicture"
            />

            <CSSTransition
                in={isVisible}
                timeout={300}
                classNames={{
                    enter: "opacity-0",
                    enterActive:
                        "opacity-100 transition-opacity duration-300 ease-in",
                    exit: "opacity-100",
                    exitActive:
                        "opacity-0 transition-opacity duration-300 ease-out",
                    exitDone: "opacity-0",
                }}
                unmountOnExit
                nodeRef={voiceBubbleRef}
                data-testid="botVoiceLine"
            >
                <div
                    className="before:bg-background relative before:absolute
                        before:top-4 before:-left-2 before:h-4 before:w-4
                        before:rotate-45 before:rounded-sm"
                    ref={voiceBubbleRef}
                >
                    <div
                        className="bg-background relative h-min max-h-full
                            overflow-auto rounded-2xl p-3 wrap-anywhere"
                    >
                        {displayVoiceLine}
                    </div>
                </div>
            </CSSTransition>
        </Card>
    );
};
export default BotVoiceLines;
