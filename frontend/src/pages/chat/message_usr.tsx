import React from "react";

import type { MessageEntry } from "./chat";

export default class Message_User extends React.Component<MessageEntry> {
    render(): React.ReactNode {
        return (
            <div className="pages_chat_message_user">
                <a>{this.props.content}</a>
            </div>
        )
    }
}