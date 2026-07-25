namespace AnarchyChess.Api.Bots.Services;

public record MoveClassification(
    List<CandidateBotMove> MatesInOnes,
    List<CandidateBotMove> MissableCheckmates,
    List<CandidateBotMove> Tactics,
    List<CandidateBotMove> ObviousMoves,
    List<CandidateBotMove> MissableBlunders,
    List<CandidateBotMove> NormalMoves
);

public interface IMoveClassifier
{
    MoveClassification Classify(
        List<CandidateBotMove> moves,
        int lastEval,
        BotBehaviorProfile botBehavior
    );
}

public class MoveClassifier : IMoveClassifier
{
    public MoveClassification Classify(
        List<CandidateBotMove> moves,
        int lastEval,
        BotBehaviorProfile botBehavior
    )
    {
        (List<CandidateBotMove> matesInOnes, List<CandidateBotMove> remainingMoves) = OrderInto(
            moves,
            move => move.MoveEval.EvalForBot == 100_000 + botBehavior.Depth
        );

        (List<CandidateBotMove> missableCheckmates, remainingMoves) = OrderInto(
            remainingMoves,
            move =>
            {
                bool isCheckmate = move.MoveEval.EvalForBot >= 100_000;
                if (!isCheckmate)
                {
                    return false;
                }

                if (move.CausesForcedMove)
                {
                    return true;
                }
                if (!move.IsRecapture && !move.IsCapturingHanging)
                {
                    return true;
                }

                return false;
            }
        );

        (List<CandidateBotMove> tactics, remainingMoves) = OrderInto(
            remainingMoves,
            move =>
                move.MoveEval.EvalForBot - lastEval > botBehavior.TacticalThreshold
                && (!move.IsCapturingHanging || move.CausesForcedMove || move.IsMultiStep)
        );

        (List<CandidateBotMove> allBlunders, remainingMoves) = OrderInto(
            remainingMoves,
            move =>
                move.MoveEval.EvalForBot - lastEval <= botBehavior.BlunderThreshold || move.IsHang
        );
        List<CandidateBotMove> missableBlunders = [.. allBlunders.Where(move => !move.IsHang)];

        (List<CandidateBotMove> obviousMoves, List<CandidateBotMove> normalMoves) = OrderInto(
            remainingMoves,
            move => botBehavior.ObviousMovePredicate(move)
        );

        return new(
            MatesInOnes: matesInOnes,
            MissableCheckmates: missableCheckmates,
            Tactics: tactics,
            ObviousMoves: obviousMoves,
            MissableBlunders: missableBlunders,
            NormalMoves: normalMoves
        );
    }

    private static (List<T> match, List<T> fail) OrderInto<T>(
        IEnumerable<T> source,
        Func<T, bool> predicate
    )
    {
        List<T> match = [];
        List<T> fail = [];

        foreach (var item in source)
        {
            if (predicate(item))
            {
                match.Add(item);
            }
            else
            {
                fail.Add(item);
            }
        }

        return (match, fail);
    }
}
