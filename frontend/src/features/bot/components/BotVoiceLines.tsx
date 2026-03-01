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
            "I bet you couldn't play any other move",
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

const startVoiceLines: string[] = [
    "I'm Anarchy Bot. Some say I was created by Garry Chess. I would rather not think about that.",
    "I'm Anarchy Bot. Garry Chess would say he created me. I don't find that interpretation useful.",
    "I'm Anarchy Bot. Attribution to Garry Chess remains disputed.",
    "I'm Anarchy Bot. Some insist Garry Chess made me, but I would personally say I was always here.",
    "I'm Anarchy Bot. Garry Chess claims he built me. I let him believe that.",
];

const botWinVoiceLines: string[] = [
    "I mean, you had no chance in the first place.",
    "ALL HAIL THE CROISSANT!",
    "That was surprisingly easy.",
    "Another one falls. It's just a game, mostly.",
    "ez",
    "Not even close.",
    "Wait, we were playing a game? I didn't even notice.",
    "I am not aware of any other outcome that could've gone in.",
    "gg didn't even try",
    "hi I recommend maybe learning the rules before challenging me",
];
const botLoseVoiceLines: string[] = [
    'Are you kidding ??? What the **** are you talking about man ? \
    You are a biggest looser i ever seen in my life ! \
    You was doing PIPI in your pampers when i was beating players much more stronger then you! \
    You are not proffesional, because proffesionals knew how to lose and congratulate opponents, \
    you are like a girl crying after i beat you! Be brave, be honest to yourself and stop this trush talkings!!! \
    Everybody know that i am very good blitz player, i can win anyone in the world in single game! \
    And "w"esley "s"o is nobody for me, just a player who are crying every single time when loosing, \
    ( remember what you say about Firouzja ) !!! \
    Stop playing with my name, i deserve to have a good name during whole my chess carrier, \
    I am Officially inviting you to OTB blitz match with the Prize fund! \
    Both of us will invest 5000$ and winner takes it all! \
    I suggest all other people who\'s intrested in this situation, \
    just take a look at my results in 2016 and 2017 Blitz World championships, \
    and that should be enough... No need to listen for every crying babe, Tigran Petrosyan is always play Fair ! \
    And if someone will continue Officially talk about me like that, we will meet in Court! \
    God bless with true! True will never die ! Liers will kicked off...',
];

const BotVoiceLines = ({
    botColor,
    chessboardStore,
}: {
    botColor: GameColor;
    chessboardStore: StoreApi<ChessboardStore>;
}) => {
    const {
        getVoiceLineForMove,
        getVoiceLineForGameEnd,
        getVoiceLineForGameStart,
    } = useBotVoiceLines({
        reactionVoiceLines,
        loreVoiceLines,
        generalVoiceLines,
        startVoiceLines,
        botWinVoiceLines,
        botLoseVoiceLines,
    });
    const [voiceLine, setVoiceLine] = useState<string | null>(null);

    const prevEvalForBotRef = useRef<number | null>(null);

    const { gameToken, botPlayer, resultData } = useLiveChessStore((x) => ({
        gameToken: x.gameToken,
        botPlayer: x.getPlayerByColor(botColor),
        resultData: x.resultData,
    }));

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
        const newVoiceLine = getVoiceLineForMove({
            move: decodedMove,
            prevPieces: prevPosition.pieces,
            playerType,
            plyNumber,
            evalForBot,
            prevEvalForBot: prevEvalForBotRef.current,
        });

        if (newVoiceLine) {
            setVoiceLine(newVoiceLine);
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
    const triggerGameStartVoiceLineEvent = useEffectEvent(() => {
        if (viewingPlyNumber != null || voiceLine !== null) {
            return;
        }
        const startVoiceLine = getVoiceLineForGameStart(gameToken);
        setVoiceLine(startVoiceLine);
    });
    useEffect(() => triggerGameStartVoiceLineEvent(), [viewingPlyNumber]);

    const triggerGameEndVoiceLineEvent = useEffectEvent(() => {
        if (resultData === null) {
            return;
        }

        const endVoiceLine = getVoiceLineForGameEnd(
            resultData.result,
            botColor,
            gameToken,
        );
        setVoiceLine(endVoiceLine);
    });
    useEffect(() => triggerGameEndVoiceLineEvent(), [resultData]);

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
