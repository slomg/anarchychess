import hyperacceleratedbongcloud1 from "@public/assets/examples/hyperacceleratedbongcloud1.png";
import hyperacceleratedbongcloud2 from "@public/assets/examples/hyperacceleratedbongcloud2.png";
import pawnpawnpromotion1 from "@public/assets/examples/pawnpawnpromotion1.png";
import pawnpawnpromotion2 from "@public/assets/examples/pawnpawnpromotion2.png";
import verticalcastling1 from "@public/assets/examples/verticalcastling1.png";
import verticalcastling2 from "@public/assets/examples/verticalcastling2.png";
import verticalcastling3 from "@public/assets/examples/verticalcastling3.png";
import forcedenpassant1 from "@public/assets/examples/forcedenpassant1.png";
import forcedenpassant2 from "@public/assets/examples/forcedenpassant2.png";
import forcedenpassant3 from "@public/assets/examples/forcedenpassant3.png";
import knooklearfusion1 from "@public/assets/examples/knooklearfusion1.png";
import knooklearfusion2 from "@public/assets/examples/knooklearfusion2.png";
import queenbetadecay1 from "@public/assets/examples/queenbetadecay1.png";
import queenbetadecay2 from "@public/assets/examples/queenbetadecay2.png";
import bouncingbishop1 from "@public/assets/examples/bouncingbishop1.png";
import bouncingbishop2 from "@public/assets/examples/bouncingbishop2.png";
import bouncingbishop3 from "@public/assets/examples/bouncingbishop3.png";
import omnipotentpawn1 from "@public/assets/examples/omnipotentpawn1.png";
import omnipotentpawn2 from "@public/assets/examples/omnipotentpawn2.png";
import omnipotentpawn3 from "@public/assets/examples/omnipotentpawn3.png";
import pawnthrowing1 from "@public/assets/examples/pawnthrowing1.png";
import pawnthrowing2 from "@public/assets/examples/pawnthrowing2.png";
import pawnthrowing3 from "@public/assets/examples/pawnthrowing3.png";
import pawnthrowing4 from "@public/assets/examples/pawnthrowing4.png";
import bishopcastle1 from "@public/assets/examples/bishopcastle1.png";
import bishopcastle2 from "@public/assets/examples/bishopcastle2.png";
import longpassant1 from "@public/assets/examples/longpassant1.png";
import longpassant2 from "@public/assets/examples/longpassant2.png";
import longpassant3 from "@public/assets/examples/longpassant3.png";
import kingcapture1 from "@public/assets/examples/kingcapture1.png";
import kingcapture2 from "@public/assets/examples/kingcapture2.png";
import ilvaticano1 from "@public/assets/examples/ilvaticano1.png";
import ilvaticano2 from "@public/assets/examples/ilvaticano2.png";
import kingtouch1 from "@public/assets/examples/kingtouch1.png";
import overtime1 from "@public/assets/examples/overtime1.png";
import overtime2 from "@public/assets/examples/overtime2.png";

import GuideCard from "@/features/guide/components/GuideCard";
import Card from "@/components/ui/Card";

const NewRulesGuide = ({ id }: { id?: string }) => {
    return (
        <Card className="scroll-mt-5 gap-5 p-5" id={id}>
            <h2 className="text-6xl">New Rules</h2>

            <hr className="text-secondary/50" />

            <GuideCard
                title="King Capture"
                points={[
                    "No check or checkmate.",
                    "You win by physically capturing the opponent's king.",
                ]}
                images={[kingcapture1, kingcapture2]}
            />

            <GuideCard
                title="King Touch = Draw"
                points={[
                    "The two kings touch (adjacent squares).",
                    "The game immediately ends in a draw.",
                ]}
                images={[kingtouch1]}
            />

            <GuideCard
                title="Pawn Throwing"
                points={[
                    "If a pawn has a higher value piece directly behind it, you can throw it in the opposite direction relative to that piece.",
                    "If the pawn hits an enemy piece, you lose your pawn but that piece is stunned for 2 moves.",
                    "If it doesn't hit anything, the pawn lands normally, but is stunned for 1 move.",
                    "Pawns can only thrown up to the second-to-last rank, they cannot be thrown to the last rank.",
                    {
                        title: "To Throw:",
                        points: [
                            "Select the pawn and move it as if capturing the piece behind it.",
                            "The screen will darken except for 3 possible trajectory lines.",
                            "One line is highlighted to indicate the current throw path.",
                            "Click the screen to cycle between available throw lines.",
                            "Right click or tap with two fingers to cancel.",
                            "Click and hold to start the throw.",
                            "A marker will move up the line, speeding up until max power, then oscillate between slightly lower and near max power.",
                            "Release to throw the pawn.",
                        ],
                    },
                ]}
                images={[
                    pawnthrowing1,
                    pawnthrowing2,
                    pawnthrowing3,
                    pawnthrowing4,
                ]}
            />

            <GuideCard
                title="Overtime"
                points={[
                    "When you run out of time, you don't instantly lose, you enter overtime.",
                    "Every few seconds, a piece on your side will get bored and leave the board.",
                    "The piece that is about to leave is highlighted. Moving that piece excites it, so it will not leave on the next removal, and the removal timer resets.",
                    "Moving any other piece does not reset the timer, and the same piece will still be pending removal next turn.",
                    "Each time you save a piece by moving it, other pieces will get jealous and the next removals will happen faster, forcing you to choose carefully which pieces to save.",
                    "Once your king leave the board, you lose the game.",
                ]}
                images={[overtime1, overtime2]}
            />

            <GuideCard
                title="Forced En Passant"
                points={["If en passant is possible, you must play it."]}
                images={[forcedenpassant1, forcedenpassant2, forcedenpassant3]}
            />

            <GuideCard
                title="Bouncing Bishop"
                points={[
                    "You can bounce your bishop off the edge of the board.",
                    "You may chain multiple bounces in a single move.",
                    "To bounce, first click your bishop, then click on the edge of the board you want to bounce off of, then you will be able to choose your bouncing path. Repeat this for multiple bounces.",
                    "You cannot bounce off other pieces.",
                    "If your bishop is able to capture an Underage Pawn through bouncing, it still must do so.",
                ]}
                images={[bouncingbishop1, bouncingbishop2, bouncingbishop3]}
            />

            <GuideCard
                title="Self-Bishop Castle Capture"
                points={[
                    "Your own bishop is blocking castling by occupying a square your king or rook would land on after castling.",
                    "You may still castle, capturing your own bishop in the process.",
                ]}
                images={[bishopcastle1, bishopcastle2]}
            />

            <GuideCard
                title="Omnipotent Pawn"
                points={[
                    "Exists on a fixed square: h3 for white, h8 for black.",
                    "If one of your pieces is captured on your Omnipotent Pawn square, you may immediately respond.",
                    "Double click the opponent piece that just captured, and your Omnipotent Pawn spawns on the square and captures it.",
                ]}
                images={[omnipotentpawn1, omnipotentpawn2, omnipotentpawn3]}
            />

            <GuideCard
                title="Long Passant"
                points={[
                    "An en passant is possible.",
                    "A diagonal chain of aligned pieces continue beyond the target pawn.",
                    "You can continue down the chain in a single move, capturing every piece in the chain.",
                    "If the chain ends on the back rank, your pawn promotes as normal.",
                ]}
                images={[longpassant1, longpassant2, longpassant3]}
            />

            <GuideCard
                title="Hyper Accelerated Bongcloud"
                points={[
                    "You can move your king one square forwad on your first turn.",
                    "This captures the pawn in front of the king.",
                    "It gives not strategic advantange except asserting dominance.",
                ]}
                images={[
                    hyperacceleratedbongcloud1,
                    hyperacceleratedbongcloud2,
                ]}
            />

            <GuideCard
                title="Pawn Pawn Promotion"
                points={[
                    "Once your pawn reaches the last rank, you can promote your pawn to a pawn",
                ]}
                images={[pawnpawnpromotion1, pawnpawnpromotion2]}
            />

            <GuideCard
                title="Il Vaticano"
                points={[
                    "There are exactly two squares between your bishops.",
                    "Two enemy pieces occupy those squares.",
                    "Your bishops can swap places and capture both enemy pieces in one move.",
                ]}
                images={[ilvaticano1, ilvaticano2]}
            />

            <GuideCard
                title="Vertical Castling"
                points={[
                    "Your king hasn't moved yet.",
                    "Your king's pawn promotes to a rook.",
                    "Since both your king and rook have not moved, you can castle vertically along the same file.",
                ]}
                images={[
                    verticalcastling1,
                    verticalcastling2,
                    verticalcastling3,
                ]}
            />

            <GuideCard
                title="Knooklear Fusion"
                points={[
                    "Your knight lands on the same square as your rook (or vice versa).",
                    "An explosion occurs, capturing every piece in a 3x3 area around them.",
                    "A knook spawns in the center of the explosion.",
                ]}
                images={[knooklearfusion1, knooklearfusion2]}
            />

            <GuideCard
                title="Queen Beta Decay"
                points={[
                    "You may split your queen into a rook, knight and a pawn by double clicking your queen if there's space.",
                    "The spawned pawn can promote like a normal pawn, but not to a queen.",
                    "The rook and knight can later perform Knooklear Fusion for massive effect.",
                ]}
                images={[queenbetadecay1, queenbetadecay2]}
            />
        </Card>
    );
};
export default NewRulesGuide;
