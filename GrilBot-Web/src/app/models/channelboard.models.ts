export class Channelboard {
    public items: ChannelboardItem[] = [];
    public guild: Guild;
    public statsFor: string;

    public static fromAny(data: any): Channelboard {
        const board = new Channelboard();

        board.items = (data.items as any[]).map(o => ChannelboardItem.fromAny(o));
        board.guild = Guild.fromAny(data.guild);
        board.statsFor = data.statsFor;

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
}

export class ChannelboardItem {
    public channelName: string;
    public count: number;

    public static fromAny(data: any): ChannelboardItem {
        const item = new ChannelboardItem();

        item.channelName = data.channelName;
        item.count = data.count;

        return item;
    }
}
