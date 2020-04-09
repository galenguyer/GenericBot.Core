import React from 'react';
import logo from './logo.svg';
import './App.css';
import Sidebar from './Routing/Sidebar';
import {
  BrowserRouter as Router,
  Switch,
  Route,
  Link
} from 'react-router-dom'

function App() {
  return (
    <Router>
      <Sidebar/>
      <div className="App">
      <Switch>
        <Route path="/testone">
          <h1>Test One Page</h1>
        </Route>
        <Route path="/">
          <h1>Home</h1>
        </Route>
      </Switch>
      </div>
    </Router>
  );
}

export default App;
