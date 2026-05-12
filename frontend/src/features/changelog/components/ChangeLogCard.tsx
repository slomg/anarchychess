import Card from "@/components/ui/Card";

export enum ChangeLogType {
    RULE,
    FIX,
    TWEAK,
    FEATURE,
    IMPROVEMENT,
    UPDATE,
}

export const CHANGELOG_TYPE_COLORS: Record<
    ChangeLogType,
    { background: string; border: string }
> = {
    [ChangeLogType.RULE]: { background: "#0F1F2E", border: "#334D61" },
    [ChangeLogType.FIX]: { background: "#0F2018", border: "#1A4028" },
    [ChangeLogType.TWEAK]: { background: "#221608", border: "#3A2A10" },
    [ChangeLogType.FEATURE]: { background: "#1A1228", border: "#2E1E48" },
    [ChangeLogType.IMPROVEMENT]: { background: "#1A0B12", border: "#3c001a" },
    [ChangeLogType.UPDATE]: { background: "#11161C", border: "#3C4A5A" },
};

const ChangeLogCard = ({
    type,
    date,
    children,
}: {
    type: ChangeLogType;
    date: string;
    children?: React.ReactNode;
}) => {
    const colors = CHANGELOG_TYPE_COLORS[type];

    return (
        <Card className="gap-2">
            <div className="flex items-center gap-2">
                <span
                    className="rounded border px-2 py-1"
                    style={{
                        backgroundColor: colors.background,
                        borderColor: colors.border,
                    }}
                >
                    {ChangeLogType[type]}
                </span>
                <span className="text-text/80">{date}</span>
            </div>

            <p>{children}</p>
        </Card>
    );
};
export default ChangeLogCard;
