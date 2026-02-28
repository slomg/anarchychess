import Image from "next/image";

const ProfilePicture = ({
    userId,
    size = 120,
    minSize,
    className,
    refreshKey,
}: {
    userId: string;
    size?: number;
    minSize?: number;
    className?: string;
    refreshKey?: number;
}) => {
    let url = `${process.env.NEXT_PUBLIC_API_URL}/api/Profile/profile-picture/${userId}`;
    if (refreshKey !== undefined) url += `?${refreshKey}`;

    return (
        <div
            className={className}
            style={{
                width: size,
                height: size,
                minWidth: minSize,
                minHeight: minSize,
            }}
        >
            <Image
                className="aspect-square rounded-md"
                src={url}
                width={size}
                height={size}
                alt="profile picture"
                data-testid="profilePicture"
                data-userid={userId}
                unoptimized
            />
        </div>
    );
};
export default ProfilePicture;
