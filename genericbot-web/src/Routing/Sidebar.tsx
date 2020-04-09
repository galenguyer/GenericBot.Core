import React from 'react';
import logo from './logo.svg';
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
        <SidebarLink name="Link One" url="/testone"/>
    </div>
  );
}

export default Sidebar;
