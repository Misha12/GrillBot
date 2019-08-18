export class Channelboard {
    public items: ChannelboardItem[] = [];
    public guild: Guild;
    public user: User;

    public static fromAny(data: any): Channelboard {
        const board = new Channelboard();

        board.items = (data.items as any[]).map(o => ChannelboardItem.fromAny(o));
        board.guild = Guild.fromAny(data.guild);
        board.user = User.fromAny(data.user);

        return board;
    }
}

export class Guild {
    public name: string;
    public avatarUrl: string;
    public usersCount: number;

    public static fromAny(data: any): Guild {
        const guild = new Guild();

        guild.avatarUrl = data.avatarUrl;
        guild.name = data.name;
        guild.usersCount = data.usersCount;

        return guild;
    }

    public getFormatedUsersCount(): string {
        if (this.usersCount === 1) { return '1 uživatel'; }
        if (this.usersCount > 1 && this.usersCount < 5) { return this.usersCount + ' uživatelé'; }

        return this.usersCount + ' uživatelů';
    }
}

export class ChannelboardItem {
    public channelName: string;
    public count: number;
    public lastMessageAt: string;

    public static fromAny(data: any): ChannelboardItem {
        const item = new ChannelboardItem();

        item.channelName = data.channelName;
        item.count = data.count;
        item.lastMessageAt = data.lastMessageAt;

        return item;
    }

    public getFormatedCount(): string {
        if (this.count === 1) { return '1 zpráva'; }
        if (this.count > 1 && this.count < 5) { return this.count + ' zprávy'; }

        return this.count + ' zpráv';
    }

    public isLastMessageLogged(): boolean {
        return this.lastMessageAt === '0001-01-01T00:00:00';
    }

    public getLocalMessageDate(): string {
        if (this.isLastMessageLogged()) {
            return 'Datum a čas poslední zprávy nedetekován.';
        }

        return new Date(this.lastMessageAt).toLocaleString();
    }
}

export class User {
    public name: string;
    public discriminator: string;
    public avatarUrl: string;
    public nickname: string;
    public status: number;

    public static fromAny(data: any): User {
        const user = new User();

        user.avatarUrl = data.avatarUrl;
        user.discriminator = data.discriminator;
        user.name = data.name;
        user.nickname = data.nickname;
        user.status = data.status;

        return user;
    }
}

export enum ErrorCodes {
    InvalidToken = 1,
    MissingToken
}
