import { Metadata } from "next";
import Link from "next/link";

import ChangeLogCard, {
    ChangeLogType,
} from "@/features/changelog/components/ChangeLogCard";

import ChangeLogMonthDivider from "@/features/changelog/components/ChangeLogMonthDivider";
import constants from "@/lib/constants";

export const metadata: Metadata = {
    title: "Change Log - Anarchy Chess",
    description: "Stay up to date with the latest Anarchy Chess updates.",
    keywords: [
        "anarchy chess updates",
        "chess variant updates",
        "new chess rules",
        "new chess features",
        "anarchychess game",
    ],
};

export default function ChangeLogPage() {
    return (
        <main className="mx-auto flex w-full max-w-7xl flex-col gap-5 p-10">
            <section className="flex flex-col gap-3">
                <h1 className="text-6xl">Change Log</h1>
                <Link
                    className="text-text/80 w-fit text-lg"
                    href={constants.PATHS.DISCORD}
                >
                    <h2>Join the Discord for near daily progress updates!</h2>
                </Link>
            </section>

            <ChangeLogMonthDivider date="May 2026" />

            <ChangeLogCard type={ChangeLogType.FIX} date="May 13">
                Fix a bug where anarchybot thought it could use stunned pieces
                to throw pawns
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FEATURE} date="May 11">
                Added a change log
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FIX} date="May 11">
                Clarified that stun pieces don&apos;t count towards traitor rook
                adjacency in the guide
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FEATURE} date="May 9">
                Added analysis position setup
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.TWEAK} date="May 9">
                Made lobotomized lobotomized anarchy bot more likely to hang
                pieces / not capture hanging pieces
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FIX} date="May 5">
                Fixed a bug where different pawn types could promote to a normal
                pawn
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.RULE} date="May 5">
                Added Queentum Tunnelling: your queen and antiqueen can swap
                places at any moment
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.TWEAK} date="May 2">
                Balanced quests that were too difficult and added a few new
                quests
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FEATURE} date="May 2">
                Added Lobotomized Lobotomized Anarchy Bot
            </ChangeLogCard>

            <ChangeLogMonthDivider date="April 2026" />

            <ChangeLogCard type={ChangeLogType.TWEAK} date="April 30">
                Bot games now count towards quests
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FEATURE} date="April 30">
                Added an &quot;all time&quot; quest leaderboard
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.IMPROVEMENT} date="April 29">
                Improved bot performance by adding a transposition table and
                iterative deepening
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.TWEAK} date="April 29">
                The game start sound is now 2x louder, and it plays before
                redirecting
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.TWEAK} date="April 23">
                Made pawn throwing no longer timing based
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.IMPROVEMENT} date="April 20">
                Added an explosion animation to knooklear fusion
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FEATURE} date="April 19">
                Added a material count to the player card
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FIX} date="April 17">
                Bots can now throw pawns
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.RULE} date="April 9">
                Added pawn throwing: you can throw your pawns up at your
                opponent and stun their pieces, or just throw your pawns
                forward.
            </ChangeLogCard>

            <ChangeLogMonthDivider date="March 2026" />

            <ChangeLogCard type={ChangeLogType.FEATURE} date="March 20">
                Added guest support to quests
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.TWEAK} date="March 19">
                Replaced ♚ and ◐ text in the bot play page with the king and
                traitor rook assets
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.RULE} date="March 17">
                Added hyper accelarated bongcloud: you can play Kf2 / Kf9 as
                your first move, capturing your own pawn in the process to
                assert dominance
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FEATURE} date="March 16">
                Added Lobotomized Anarchy Bot
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FEATURE} date="March 5">
                Added navigation buttons to move history
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.TWEAK} date="March 5">
                Added a typing animation to bot dialog
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FEATURE} date="March 3">
                Added Anarchy Bot
            </ChangeLogCard>

            <ChangeLogMonthDivider date="Feburary 2026" />

            <ChangeLogCard type={ChangeLogType.UPDATE} date="Feburary 23">
                Finished backend and frontend for Anarchy Bot, the only things
                left are voice lines and a profile picture
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.TWEAK} date="Feburary 18">
                Profile game history now shows the time control of each game
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.UPDATE} date="Feburary 18">
                Anarchy Bot engine is close to done. I still need to integrate
                it with the website
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.UPDATE} date="Feburary 9">
                Finished implementing all pieces with bitboards for the Anarchy
                Bot engine
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.UPDATE} date="Feburary 2">
                Started working on Anarchy Bot
            </ChangeLogCard>

            <ChangeLogMonthDivider date="January 2026" />

            <ChangeLogCard type={ChangeLogType.TWEAK} date="January 31">
                Moved traitor rooks from a5 and j6 to a7 and j4 so it&apos;s
                fairer for black
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.TWEAK} date="January 30">
                Clocks now show how much time at the time you had left when you
                back in move history after the game is over
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.RULE} date="January 28">
                Added overtime: after you run out of time, instead of instantly
                losing, your pieces start getting bored and leave the board one
                by one until your king leaves
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.RULE} date="January 20">
                Added bouncing bishops: bishops can now bounce off walls
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FIX} date="January 19">
                Fixed some special moves not working on the analysis board
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FEATURE} date="January 19">
                Added omnipotent pawn indicator
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FEATURE} date="January 17">
                Added grace period before your clock starts
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FIX} date="January 14">
                Fixed various clock desync issues
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FEATURE} date="January 12">
                ANARCHY CHESS HAS BEEN RELEASED!
            </ChangeLogCard>
        </main>
    );
}
