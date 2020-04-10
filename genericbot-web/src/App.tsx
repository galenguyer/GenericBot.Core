import React from 'react';
import './App.css';
import Sidebar from './Routing/Sidebar';
import Quote from './Quotes/Quote';
import {
  BrowserRouter as Router,
  Switch,
  Route,
} from 'react-router-dom'

function App() {
  return (
    <Router>
      <Sidebar/>
      <div className="App">
      <Switch>
        <Route path="/quotes">
          <Quote quoteId={2} content="Funny quote haha - me lol" admin={true}/>
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
