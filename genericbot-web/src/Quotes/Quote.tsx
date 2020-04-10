import React from 'react';
import './Quote.css';

export interface QuoteData {
    quoteId: number,
    content: string,
    admin: boolean
}

class Quote extends React.Component<QuoteData, {}> {
    render() {
        const {quoteId, content, admin} = this.props;
        var button = null;
        var contentName = "QuoteContent"
        if(admin){
            button = <button className="DeleteButton">Delete</button>
            contentName = "QuoteContent Admin"
        }
        return (
            <div className="Quote">
                <span className="QuoteIdBox"><span className="QuoteId">{quoteId}</span></span>
                <span className={contentName}>{content}</span>
                {button}
            </div>
        );
    }
}

export default Quote;
