import useChallengeStore from "../../hooks/useChallengeStore";

const ChallengeStatusText = ({
    activeText,
    activeClassName,
    overClassName,
}: {
    activeText: string;
    activeClassName: string;
    overClassName: string;
}) => {
    const isExpired = useChallengeStore((x) => x.isExpired);
    const challenge = useChallengeStore((x) => x.challenge);

    let text: string;
    let className: string;

    if (isExpired) {
        text = "Challenge Expired";
        className = overClassName;
    } else if (challenge.cancelledBy) {
        text =
            challenge.cancelledBy === challenge.recipient?.userId
                ? "Challenge Declined"
                : "Challenge Cancelled";
        className = overClassName;
    } else {
        text = activeText;
        className = activeClassName;
    }

    return (
        <p className={className} data-testid="challengeStatusText">
            {text}
        </p>
    );
};
export default ChallengeStatusText;
