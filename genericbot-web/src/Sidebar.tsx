import React from 'react';
import logo from './logo.svg';
import './Sidebar.css';
import SidebarLink from './SidebarLink';

function Sidebar() {
  return (
    <div className="Sidebar">
        <h1>GenericBot</h1>
        <h3>Web Dashboard</h3>
        <hr />
        <SidebarLink name="Link One" url="/testone"/>
    </div>
  );
}

export default Sidebar;
