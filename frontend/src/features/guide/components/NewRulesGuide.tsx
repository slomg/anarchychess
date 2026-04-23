import hyperAcceleratedBongcloud1 from "@public/assets/examples/hyper-accelerated-bongcloud1.png";
import hyperAcceleratedBongcloud2 from "@public/assets/examples/hyper-accelerated-bongcloud2.png";
import pawnPawnPromotion1 from "@public/assets/examples/pawn-pawn-promotion1.png";
import pawnPawnPromotion2 from "@public/assets/examples/pawn-pawn-promotion2.png";
import verticalCastling1 from "@public/assets/examples/vertical-castling1.png";
import verticalCastling2 from "@public/assets/examples/vertical-castling2.png";
import verticalCastling3 from "@public/assets/examples/vertical-castling3.png";
import forcedEnPassant1 from "@public/assets/examples/forced-en-passant1.png";
import forcedEnPassant2 from "@public/assets/examples/forced-en-passant2.png";
import forcedEnPassant3 from "@public/assets/examples/forced-en-passant3.png";
import knooklearFusion1 from "@public/assets/examples/knooklear-fusion1.png";
import knooklearFusion2 from "@public/assets/examples/knooklear-fusion2.png";
import queenBetaDecay1 from "@public/assets/examples/queen-beta-decay1.png";
import queenBetaDecay2 from "@public/assets/examples/queen-beta-decay2.png";
import bouncingBishop1 from "@public/assets/examples/bouncing-bishop1.png";
import bouncingBishop2 from "@public/assets/examples/bouncing-bishop2.png";
import bouncingBishop3 from "@public/assets/examples/bouncing-bishop3.png";
import omnipotentPawn1 from "@public/assets/examples/omnipotent-pawn1.png";
import omnipotentPawn2 from "@public/assets/examples/omnipotent-pawn2.png";
import omnipotentPawn3 from "@public/assets/examples/omnipotent-pawn3.png";
import pawnThrowing1 from "@public/assets/examples/pawn-throwing1.png";
import pawnThrowing2 from "@public/assets/examples/pawn-throwing2.png";
import pawnThrowing3 from "@public/assets/examples/pawn-throwing3.png";
import pawnThrowing4 from "@public/assets/examples/pawn-throwing4.png";
import bishopCastle1 from "@public/assets/examples/bishop-castle1.png";
import bishopCastle2 from "@public/assets/examples/bishop-castle2.png";
import longPassant1 from "@public/assets/examples/long-passant1.png";
import longPassant2 from "@public/assets/examples/long-passant2.png";
import longPassant3 from "@public/assets/examples/long-passant3.png";
import kingCapture1 from "@public/assets/examples/king-capture1.png";
import kingCapture2 from "@public/assets/examples/king-capture2.png";
import ilVaticano1 from "@public/assets/examples/il-vaticano1.png";
import ilVaticano2 from "@public/assets/examples/il-vaticano2.png";
import kingTouch1 from "@public/assets/examples/king-touch1.png";
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
                images={[kingCapture1, kingCapture2]}
            />

            <GuideCard
                title="King Touch = Draw"
                points={[
                    "The two kings touch (adjacent squares).",
                    "The game immediately ends in a draw.",
                ]}
                images={[kingTouch1]}
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
                            "3 trajectory lines will appear, one will be highlighted.",
                            "Click or swipe sideways (perpendicular to the throw direction) to change trajectory.",
                            "Scroll wheel or swipe up / down (relative to the throw direction) to change distance.",
                            "Hold click to confirm your throw.",
                            "Right click or 2-finger tap to cancel.",
                        ],
                    },
                ]}
                images={[
                    pawnThrowing1,
                    pawnThrowing2,
                    pawnThrowing3,
                    pawnThrowing4,
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
                images={[forcedEnPassant1, forcedEnPassant2, forcedEnPassant3]}
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
                images={[bouncingBishop1, bouncingBishop2, bouncingBishop3]}
            />

            <GuideCard
                title="Self-Bishop Castle Capture"
                points={[
                    "Your own bishop is blocking castling by occupying a square your king or rook would land on after castling.",
                    "You may still castle, capturing your own bishop in the process.",
                ]}
                images={[bishopCastle1, bishopCastle2]}
            />

            <GuideCard
                title="Omnipotent Pawn"
                points={[
                    "Exists on a fixed square: h3 for white, h8 for black.",
                    "If one of your pieces is captured on your Omnipotent Pawn square, you may immediately respond.",
                    "Double click the opponent piece that just captured, and your Omnipotent Pawn spawns on the square and captures it.",
                ]}
                images={[omnipotentPawn1, omnipotentPawn2, omnipotentPawn3]}
            />

            <GuideCard
                title="Long Passant"
                points={[
                    "An en passant is possible.",
                    "A diagonal chain of aligned pieces continue beyond the target pawn.",
                    "You can continue down the chain in a single move, capturing every piece in the chain.",
                    "If the chain ends on the back rank, your pawn promotes as normal.",
                ]}
                images={[longPassant1, longPassant2, longPassant3]}
            />

            <GuideCard
                title="Hyper Accelerated Bongcloud"
                points={[
                    "You can move your king one square forwad on your first turn.",
                    "This captures the pawn in front of the king.",
                    "It gives not strategic advantange except asserting dominance.",
                ]}
                images={[
                    hyperAcceleratedBongcloud1,
                    hyperAcceleratedBongcloud2,
                ]}
            />

            <GuideCard
                title="Pawn Pawn Promotion"
                points={[
                    "Once your pawn reaches the last rank, you can promote your pawn to a pawn",
                ]}
                images={[pawnPawnPromotion1, pawnPawnPromotion2]}
            />

            <GuideCard
                title="Il Vaticano"
                points={[
                    "There are exactly two squares between your bishops.",
                    "Two enemy pieces occupy those squares.",
                    "Your bishops can swap places and capture both enemy pieces in one move.",
                ]}
                images={[ilVaticano1, ilVaticano2]}
            />

            <GuideCard
                title="Vertical Castling"
                points={[
                    "Your king hasn't moved yet.",
                    "Your king's pawn promotes to a rook.",
                    "Since both your king and rook have not moved, you can castle vertically along the same file.",
                ]}
                images={[
                    verticalCastling1,
                    verticalCastling2,
                    verticalCastling3,
                ]}
            />

            <GuideCard
                title="Knooklear Fusion"
                points={[
                    "Your knight lands on the same square as your rook (or vice versa).",
                    "An explosion occurs, capturing every piece in a 3x3 area around them.",
                    "A knook spawns in the center of the explosion.",
                ]}
                images={[knooklearFusion1, knooklearFusion2]}
            />

            <GuideCard
                title="Queen Beta Decay"
                points={[
                    "You may split your queen into a rook, knight and a pawn by double clicking your queen if there's space.",
                    "The spawned pawn can promote like a normal pawn, but not to a queen.",
                    "The rook and knight can later perform Knooklear Fusion for massive effect.",
                ]}
                images={[queenBetaDecay1, queenBetaDecay2]}
            />
        </Card>
    );
};

export default NewRulesGuide;
