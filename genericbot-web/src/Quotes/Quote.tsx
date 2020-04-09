import React from 'react';
import './Quote.css';

export interface QuoteData {
    quoteId: number,
    content: string,
    submitter: boolean,
    admin: boolean
}

class Quote extends React.Component<QuoteData, {}> {
    render() {
        const {quoteId, content, submitter, admin} = this.props;
        return (
            <div className="Quote">
                <p>This is a quote</p>
            </div>
        );
    }
}

export default Quote;
