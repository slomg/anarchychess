using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.Vote.Entities;
using AnarchyChess.Api.Vote.Repositories;

namespace AnarchyChess.Api.Vote.Services;

public interface IVoteSeeder
{
    Task SeedAsync(CancellationToken token = default);
}

public class VoteSeeder(IVoteRepository voteRepository, IUnitOfWork unitOfWork) : IVoteSeeder
{
    private readonly List<VoteOption> _options =
    [
        new()
        {
            Key = "you-will-never-take-me-alive",
            Name = "You Will Never Take Me Alive!",
            Description =
                "Kings are explosives, killing everything around them when taken. Useless on its own, it makes secondary kings VERY valuable.",
        },
        new()
        {
            Key = "horsey-legacy",
            Name = "Horsey's Legacy",
            Description =
                "Horseys drop their their sword on death. When a pawn (your color or not) lands on the sword, it will follow the horsey's legacy and become the horsey themselves. The newely created horsey will have the same color as the horsey that died.",
        },
        new()
        {
            Key = "mister-president-noooooooooo",
            Name = "Mister president, Noooooooooo",
            Description =
                "Available pieces can block a lethal move on the king if it can be blocked by their basic movement (like a bishop or knight blocking a rook path). The king gets stunned for 1-2 turns as shock compensation.",
        },
        new()
        {
            Key = "consacred-ground",
            Name = "Consacred Ground",
            Description =
                "Where a bishop dies becomes a permanent halo zone. Capturing a piece in the halo smites and kills the capturing piece as well.",
        },
        new()
        {
            Key = "french-revolution",
            Name = "French Revolution",
            Description =
                "You can kill your king and queen and choose another piece to become a leader. The new leader will move like a king, regardless of the piece they were before.",
        },
        new()
        {
            Key = "the-tower",
            Name = "The Tower",
            Description =
                "Double clicking a rook turns it into an immovable, invincible tower that blocks all movement. Towers can be used as bouncing platforms for bishops. Traitor rooks can also become towers if fully controlled.",
        },
        new()
        {
            Key = "air-hockey",
            Name = "Air Hockey",
            Description =
                "Checkers act as hockey pucks. Sliding a piece into a checker gives it force in that direction, pushing it across the board. It moves until force runs out, bouncing off walls and pushing pieces in its path.",
        },
        new()
        {
            Key = "king-passant",
            Name = "King Passant",
            Description =
                "Kings may be captured en passant. If your king castles out of through a check, your opponent can capture the king by playing any move to the square it left or moved through.",
        },
        new()
        {
            Key = "sniper-bishops",
            Name = "Sniper Bishops",
            Description =
                "Bishops can promote into a sniper bishop upon reaching the end of the board. Sniper bishops can no longer bounce but can capture without moving, and must reload (stunned for 1 turn) after each shot.",
        },
        new()
        {
            Key = "der-wagentanz",
            Name = "Der Wagentanz",
            Description =
                "If a rook reaches the center 4 squares, it can turn into a disco ball. This stuns all enemy pieces for 1 turn, and any enemy piece landing on the same row or column of the disco ball is stunned for 1 turn.",
        },
        new()
        {
            Key = "la-fortaleza-del-rey",
            Name = "La Fortaleza Del Rey",
            Description =
                "If the king has not moved and the four middle squares of the second rank are free, the king can place a wall on each of those squares. Walls cannot move and cannot be captured.",
        },
        new()
        {
            Key = "pawn-stacking",
            Name = "Pawn Stacking",
            Description =
                "Pawns can be placed on top of other pawns. Clicking a stack moves the entire stack, while double-clicking moves only the top pawn. If a stack is captured, all pawns in it die. A stack cannot reach the last rank and must be disassembled before promotion.",
        },
        new()
        {
            Key = "icy-spaces",
            Name = "Icy Spaces",
            Description =
                "Every 10 moves, a random square becomes icy. Pieces sliding over it keep moving in their current direction until blocked or off-board, jumping pieces landing on it are stunned for 1 turn. After 3 uses, the icy square disappears.",
        },
        new()
        {
            Key = "domain-expansion",
            Name = "Domain Expansion",
            Description =
                "The first time a king lands on one of the center 4 squares, all pieces in the central 6x6 area are stunned for 3 turns (friendly pieces for 2 turns).",
        },
        new()
        {
            Key = "leapfrog",
            Name = "Leapfrog",
            Description =
                "If a pawn is behind another piece and there is an empty space in front of that piece, it may jump over it without capturing. This move can be chained.",
        },
        new()
        {
            Key = "adultery-prevention-law",
            Name = "Adultery Prevention Law",
            Description =
                "If two queens of the same color can see each other (as if they could capture if opposite colors), they must capture each other.",
        },
        new()
        {
            Key = "pawn-breeding",
            Name = "Pawn Breeding",
            Description =
                "If a pawn is directly behind another, it can move onto it, making the pawn in front pregnant. The pregnant pawn is stunned for 9 moves, then spawns an underaged pawn in front of it. 10% chance of miscarriage.",
        },
        new()
        {
            Key = "yuri-meter",
            Name = "The Yuri Meter",
            Description =
                "Each turn where both queens are in each other's line of sight increases the meter by 1. If they are not, it decreases by 1. At 6, both players lose.",
        },
        new()
        {
            Key = "super-checker",
            Name = "OH BABY A TRIPLE, OH YEAH",
            Description =
                "If a checker gets a triple kill, it gains +676767 aura and becomes a super checker, increasing its movement range to 3 squares instead of 2.",
        },
        new()
        {
            Key = "jester-piece",
            Name = "Jester Piece",
            Description = "Moves like the last piece that moved.",
        },
        new()
        {
            Key = "hentaivirus",
            Name = "Hentaivirus",
            Description =
                "One pawn is infected with the hentai-virus. Any piece adjacent to the pawn is infected, and their right arm becomes occupied for 3 turns. This means they can't capture.",
        },
        new()
        {
            Key = "royalfission",
            Name = "Royalfission",
            Description =
                "Moving a king onto another fuses them together, taking all non-king pieces in a 5x5 area and gives the king an extra life.",
        },
        new()
        {
            Key = "tomtsktwalmicaykhttttiwarm",
            Name = "TOMTSKTWALMICAYKHTTTTIWARM",
            Description =
                "(That one move that some kids thought was a legal move in chess and you kept having to tell them that it wasn't a real move) If both your queen and king haven't moved, you can swap their places by moving one onto the other.",
        },
        new()
        {
            Key = "big-back-rooks",
            Name = "Big Back Rooks",
            Description =
                "When a rook moves, the square it moved from becomes unsturdy for 1 turn. Any piece moving onto or over it falls through the board.",
        },
        new()
        {
            Key = "rook-sweep",
            Name = "Rook Sweep",
            Description =
                "If a rook moves an entire rank uninterrupted, all pieces on adjacent ranks are pushed 1 space in the same direction.",
        },
        new()
        {
            Key = "stone-wall",
            Name = "Stone Wall",
            Description =
                "Obtained from fusing a queen and a checker, it can move without capturing like a queen ONCE, after that, it cant move, capture, or be captured in any way.",
        },
        new()
        {
            Key = "elle-eurasia",
            Name = "Elle Eurasia (Diagonal Il Vaticano)",
            Description = "Allow il vaticano diagonally using rooks.",
        },
        new()
        {
            Key = "sqrt-minus-1-file",
            Name = "Sqrt(-1) File",
            Description = "Rename the i file to the sqrt(-1) file.",
        },
        new()
        {
            Key = "fisher-random",
            Name = "Fisher Random",
            Description = "Clicking the king on the first rank shuffles pieces on that rank.",
        },
        new()
        {
            Key = "opponent-promotion",
            Name = "Opponent Promotion",
            Description = "You can promote your pawn into an opponent piece.",
        },
        new()
        {
            Key = "minus-1-over-12-time-control",
            Name = "-1/12 Time Control",
            Description =
                "First move has 1 second, second move has 2 seconds, third has 3 seconds, and so on. Time increases each turn indefinitely, basically you will never run out of time and it kinda becomes correspondence.",
        },
        new()
        {
            Key = "loyal-heir",
            Name = "Loyal Heir",
            Description =
                "If a underage pawn is adjacent to a king or queen when one of them dies died, that pawn becomes a king/queen",
        },
        new()
        {
            Key = "hyper-hyper-accelerated-bongcloud",
            Name = "Hyper Hyper Accelerated bongcloud",
            Description =
                "When doing Hyper Accelerated Bongcloud, the king is able to move forward an additional square for even more of a flex.",
        },
        new()
        {
            Key = "traitor-rook-fusion",
            Name = "Traitor Rook Fusion",
            Description =
                "Capturing a traitor rook with another traitor rook creates an unmovable, uncapturable tower.",
        },
        new()
        {
            Key = "king-touch-extension",
            Name = "King Touch Extension",
            Description =
                "Making a move (including promotion) that leads up to two of your kings touching results in an immediate loss. Making a move (including promotion) such that any of your kings touches more than one of the opponent's kings results in an immediate win.",
        },
        new()
        {
            Key = "fast-fianchetto",
            Name = "Fast Fianchetto",
            Description = "Moving a bishop onto the c or h pawn automatically fianchettos it.",
        },
        new()
        {
            Key = "king-ascension",
            Name = "King Ascension",
            Description =
                "If your only king reaches the end of the board, it ascends into godhood, becoming immune to damage for 1 turn each time it captures a piece. Instead of capturing, attacking pieces die. You cannot ascend while you have 2 or more kings. King touch still applies.",
        },
        new()
        {
            Key = "bishop-smash",
            Name = "Bishop Smash",
            Description =
                "When a bishop bounces and ends up on the same square it started from, it smashes into the square, permanently removing it and pushing adjacent pieces one square away. The bishop has 1 turn to move off the broken square or it falls and is captured.",
        },
        new()
        {
            Key = "911-gambit",
            Name = "9/11 Gambit",
            Description =
                "If both of your opponent's rooks can be captured by a single piece, you can capture both at the same time.",
        },
    ];

    private readonly IVoteRepository _voteRepository = voteRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task SeedAsync(CancellationToken token = default)
    {
        await _voteRepository.BulkAddVoteOptionsIfNotExistAsync(_options, token);

        List<VoteOptionPair> pairs = [];
        for (int i = 0; i < _options.Count; i++)
        {
            var optionA = _options[i];
            for (int j = i + 1; j < _options.Count; j++)
            {
                var optionB = _options[j];
                pairs.Add(
                    new VoteOptionPair()
                    {
                        OptionAKey = optionA.Key,
                        OptionA = optionA,
                        OptionBKey = optionB.Key,
                        OptionB = optionB,
                    }
                );
            }
        }

        await _voteRepository.BulkAddVoteOptionPairsIfNotExistAsync(pairs, token);
        await _unitOfWork.CompleteAsync(token);
    }
}
