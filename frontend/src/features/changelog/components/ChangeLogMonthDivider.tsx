const ChangeLogMonthDivider = ({ date }: { date: string }) => {
    return (
        <div className="flex w-full flex-col gap-2">
            <p className="text-text/60">{date}</p>
            <hr className="text-secondary/30" />
        </div>
    );
};
export default ChangeLogMonthDivider;
