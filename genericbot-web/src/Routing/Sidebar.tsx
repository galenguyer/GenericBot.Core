import React from 'react';
import './Sidebar.css';
import SidebarLink from './SidebarLink';
import { Link } from 'react-router-dom';
 
function Sidebar() {
  return (
    <div className="Sidebar">
      <Link to="/">  
        <h1>GenericBot</h1>
        <h3>Web Dashboard</h3>
      </Link>
      <hr />
        <SidebarLink name="Quotes" url="/quotes"/>
    </div>
  );
}

export default Sidebar;
