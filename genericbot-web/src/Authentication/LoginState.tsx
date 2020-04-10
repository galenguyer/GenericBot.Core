import React from 'react';
import './LoginState.css';
import Cookies from 'universal-cookie';
import { Link } from 'react-router-dom';

export default function LoginState(){
    const cookies = new Cookies();
    var loginMessage: any = undefined;
    if(cookies.get("Authorization")==undefined) {
        loginMessage = (
            <Link to="/login">
                <p>not logged in</p>
            </Link>
            )
    } else {
        loginMessage = <p>logged in</p>
    }

    return(
        <div className="LoginState">
        <hr/>
        {loginMessage}
        </div>
    )
}