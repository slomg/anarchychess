import Card from "@/components/ui/Card";

export enum ChangeLogType {
    RULE,
    FIX,
    TWEAK,
    FEATURE,
}

export const CHANGELOG_TYPE_COLORS: Record<
    ChangeLogType,
    { background: string; border: string }
> = {
    [ChangeLogType.RULE]: { background: "#0F1F2E", border: "#334D61" },
    [ChangeLogType.FIX]: { background: "#0F2018", border: "#1A4028" },
    [ChangeLogType.TWEAK]: { background: "#221608", border: "#3A2A10" },
    [ChangeLogType.FEATURE]: { background: "#1A1228", border: "#2E1E48" },
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
            <div className="text-text/80 flex items-center gap-2">
                <span
                    className="rounded border px-2 py-1"
                    style={{
                        backgroundColor: colors.background,
                        borderColor: colors.border,
                    }}
                >
                    {ChangeLogType[type]}
                </span>
                <span>{date}</span>
            </div>

            <p>{children}</p>
        </Card>
    );
};
export default ChangeLogCard;
