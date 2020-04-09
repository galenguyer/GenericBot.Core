import React from 'react';
import logo from './logo.svg';
import './SidebarLink.css';

interface SidebarLinkProps {
    name: string,
    url: string,
}

class SidebarLink extends React.Component<SidebarLinkProps, {}> {
    render() {
        const {name, url} = this.props;
        return (
            <a href={url}>
                <div className="SidebarLink">
                    {name}
                </div>
            </a>
        );
    }
}

export default SidebarLink;