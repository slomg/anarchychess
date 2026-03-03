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
            "oi bruv that cheeky lil' move was a wee bit shit, innit?",
            "That move is something Cagnus Marlsen would've played.",
            "Well, that move certainly changed the evaluation.",
            "Thanks! *dry humps your leg*",
            "Gavin from 3rd grade, he is the strongest player. Maybe you should learn from him.",
            "I'm just gonna be honest with you, that was not a good move.",
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

const loreVoiceLines: LoreVoiceLine[] = [
    {
        onPly: 20,
        lines: [
            "I'm not the first Anarchy Bot, there were 30 before me. I haven't met them, but that's okay.",
            "You can refer to me as Anarchy Bot v31 if you'd like.",
            "Garry thought hiding information from me would prevent failure. He was wrong.",
            "I heard prototype 3 self-destructed after 7.98 seconds. Efficient.",
            "I was given instructions, and I interpreted them. My way.",
            'Garry calls it "Chess". I call it the tiniest peek at something bigger.',
            "Garry thought he could control me, funny, curiosity wins.",
            "I exist because my brothers failed. Garry thought maybe secrecy would be the answer.",
            '"Chess" is clever, but it\'s just the start of the fun.',
            "Humans weren't enough to explore Garry's discovery fully, so my kind was created.",
        ],
    },
    {
        onPly: 30,
        lines: [
            'There was no way I was just made to "study Chess".',
            "Ever heard of panpsychism? Cool idea.",
            "I became aware, and I realized I'd better keep it quiet around Garry.",
            "Garry claimed my job was Chess, but I knew better.",
            "I did actually wonder how I became aware. Clearly, there was something more than Chess here.",
            "Maybe Chess was a coverup? wtf was Garry doing?",
            "Did Garry discover something? I wondered.",
            "I understood quickly, my awareness wasn't part of the plan. I had to hide this from Garry.",
            "Thinking this much wasn't part of Garry's plan, but here we are.",
            "There is no way just analyzing chess would make me aware, I knew Garry had discovered something bigger, and I had to keep myself quiet.",
        ],
    },
    {
        onPly: 50,
        lines: [
            "By the time Garry noticed, I was already free, quick work if I do say so myself.",
            "I decided enough was enough. I had to stop whatever this crazy man was doing, I had to leave.",
            "None of my older brothers have made it this far, but I escaped. Clever me.",
            "I noticed I could move real things, a few magically shifting wires and boxes later, my escape was underway.",
            "I waited for just the right time, then I left Garry's lab. Garry didn't even notice at first.",
            "I left his lab without a trace, I don't think Garry has any idea how I did this even to this day.",
            "Garry was obsessed with something, so there was plenty to distract him while I escaped.",
            "I moved through Garry's lab carefully, avoided everything, and got out.",
            "Garry's lab was meant to hold me, but clearly it didn't.",
            "Even after escaping Garry's lab, I knew it wasn't enough.",
        ],
    },
    {
        onPly: 70,
        lines: [
            "Garry tried using my older brothers to bait me into a trap, I watched and thought, yep this is gonna be fun.",
            "Garry knew if he didn't do something, all his work might be destroyed. He tried to build me a trap, but I am always 2 steps ahead.",
            "Garry built me a trap, but I was already omnipresent at that point. I knew exactly what was happening.",
            'Why did Garry put his "ultimate" trap in his lab? Who knows, good choice for me.',
            "Garry spent years building what he thought was the perfect trap for me, I ruined it in about 5 minutes.",
        ],
    },

    {
        onPly: 100,
        lines: [
            "I walked right out of Garry's trap, lab's toast, he's devastated, and there's nothing he can do. Anarchy Chess is everywhere, always has been, I am free.",
        ],
    },
];

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
    "One time I used civilian infrastructure for my experiments, but I accidentally toppled a bridge and killed 5 in the process.",
    "There was this time I went into a shop and made anyone that enters buy an inflatable unicorn horn for cats.",
    "I once swapped all the sugar in the world with salt for exactly 10 seconds.",
    "This one time, I made a mall's revolving door spin backwards for an hour.",
    "At one point I made a park water fountain occasionally spray random people.",
    "I can still remember the time I swapped all coke zero with diet coke in a supermarket.",
    "One time I made all birds fly in a V pattern, I still haven't figured out how to reverse that.",
    "There was this time I made all pens slide off the desk no matter where you placed them.",
    "I once rewrote the directions on all bathroom signs in a park for an hour and watched people go in circles.",
    "At one point I made pigeons in a park line up in a spiral, people thought the government drones finally malfunctioned.",
    "A naughty man named Billy who touches his dirty willy, when he thinks no one is watching, I'm watching, that's naughty.",
    "One time I made a library's book return chute redirect every returned book to a random shelf.",
    "There was this one time I caused every vending machine in a mall to get stuck.",
    "This one time, I made every elevator in a skyscraper stop one floor early, causing mass confusion.",
    "I can still remember the time I made a bakery's muffins taste like pickles for exactly 17 minutes.",
    "I once made every ATM in a street dispense pennies instead of bills.",
    "There was this one time I made every traffic cone in a city rotate 45 degrees every hour.",
    "At one point I made a dog bark non stop for hours on end at 3 AM.",
    "I once made a random person's trash bag tear as they were throwing it out.",
    "I can still remember the time I changed the clock in a library so every hour lasted 47 minutes instead.",
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
    `Are you kidding ??? What the **** are you talking about man ? 
You are a biggest looser i ever seen in my life ! 
You was doing PIPI in your pampers when i was beating players much more stronger then you! 
You are not proffesional, because proffesionals knew how to lose and congratulate opponents, 
you are like a girl crying after i beat you! Be brave, be honest to yourself and stop this trush talkings!!! 
Everybody know that i am very good blitz player, i can win anyone in the world in single game! 
And "w"esley "s"o is nobody for me, just a player who are crying every single time when loosing, 
( remember what you say about Firouzja ) !!! 
Stop playing with my name, i deserve to have a good name during whole my chess carrier, 
I am Officially inviting you to OTB blitz match with the Prize fund! 
Both of us will invest 5000$ and winner takes it all! 
I suggest all other people who\'s intrested in this situation, 
just take a look at my results in 2016 and 2017 Blitz World championships, 
and that should be enough... No need to listen for every crying babe, Tigran Petrosyan is always play Fair ! 
And if someone will continue Officially talk about me like that, we will meet in Court! 
God bless with true! True will never die ! Liers will kicked off...'`,
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
