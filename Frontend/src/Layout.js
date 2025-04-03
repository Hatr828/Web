import { Link, Outlet } from "react-router-dom";
import './Layout.css';
import { useContext, useState } from "react";
import { AppContext } from "./App";

export default function Layout(){
    return <>
        <div className="page-container">
            <header>
            <nav className="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3">
                <div className="container-fluid">
                    <Link to='/' className="navbar-brand">Крамниця</Link>
                    <button className="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target=".navbar-collapse" aria-controls="navbarSupportedContent"
                            aria-expanded="false" aria-label="Toggle navigation">
                        <span className="navbar-toggler-icon"></span>
                    </button>
                    <div className="navbar-collapse collapse d-sm-inline-flex justify-content-between">
                        <ul className="navbar-nav flex-grow-1">
                            <li className="nav-item">
                                <Link to='/' className="nav-link text-dark">Домашня</Link>
                            </li>
                            <li className="nav-item">
                                <Link className="nav-link text-dark">Угода</Link>
                            </li>
                        </ul>
                        <div className="user-block">

                                <button type="button" className="btn btn-outline-primary" data-bs-toggle="modal" data-bs-target="#authModal">
                                    <i className="bi bi-box-arrow-in-right"></i>
                                </button>
                                <Link to='/' className="btn btn-outline-dark"><i className="bi bi-person-plus"></i></Link>
                        </div>
                    </div>
                </div>
            </nav>
            </header>

            <section className="container">
                <main role="main" className="pb-3">
                    <Outlet />
                </main>
            </section>
            
            <footer className="border-top footer text-muted">
                <div className="container">
                    &copy; 2024 - ASP-P22 - <Link className="nav-link text-dark">Угода</Link>
                </div>
            </footer>

            <AuthModal/>
        </div>
    </>;

function AuthModal(){
    const { request } = useContext(AppContext);

    const [login, setLogin] = useState('');
    const [password, setPassword] = useState('');
    const [errors, setErrors] = useState({ login: "", password: "" });

    const validate = (e) => {
        const { name, value } = e.target;
        let error = "";
    
        if (value.trim() === "") {
            error = "Поле не може бути пустим.";
        } else if (name === "login" && !/^[A-ZА-Я]/.test(value)) {
            error = "Логін повинен починатися з великої літери.";
        } else if (name === "password" && (value.length < 8 || value.length > 16)) {
            error = "Пароль повинен бути від 8 до 16 символів.";
        }
    
        setErrors((prev) => ({
            ...prev,
            [name]: error
        }));
    };

    const isValid = login.trim() !== "" && password.trim() !== "" && !errors.login && !errors.password;

    const authenticateClick = () => {
        if (!isValid) return;

        const credentials = btoa(login + ':' + password);
        request("/api/user", {
            method: "GET",
            headers: {
                'Authorization': 'Basic ' + credentials
            }
        }).then(console.log).catch(console.error);
    };

    return (
        <div className="modal fade" id="authModal" tabIndex="-1" aria-labelledby="authModalLabel" aria-hidden="true">
            <div className="modal-dialog">
                <div className="modal-content">
                    <div className="modal-header">
                        <h1 className="modal-title fs-5" id="authModalLabel">Автентифікація</h1>
                        <button type="button" className="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div className="modal-body">
                        <form id="auth-form">
                            <div className="input-group mb-3">
                                <span className="input-group-text" id="auth-login-addon">
                                    <i className="bi bi-key"></i>
                                </span>
                                <input 
                                    value={login} name="login" type="text" 
                                    className={`form-control ${errors.login ? "is-invalid" : login ? "is-valid" : ""}`} 
                                    onChange={e => { setLogin(e.target.value); validate(e); }} onBlur={validate} 
                                    placeholder="Логін" aria-label="Login" aria-describedby="auth-login-addon"
                                />
                                <div className="invalid-feedback">{errors.login}</div>
                            </div>
                            <div className="input-group mb-3">
                                <span className="input-group-text" id="auth-password-addon">
                                    <i className="bi bi-lock"></i>
                                </span>
                                <input 
                                    value={password} name="password" type="password" 
                                    className={`form-control ${errors.password ? "is-invalid" : password ? "is-valid" : ""}`} 
                                    onChange={e => { setPassword(e.target.value); validate(e); }} onBlur={validate} 
                                    placeholder="Пароль" aria-label="Password" aria-describedby="auth-password-addon"
                                />
                                <div className="invalid-feedback">{errors.password}</div>
                            </div>
                        </form>
                    </div>
                    <div className="modal-footer">
                        <div id="auth-error" className="text-danger me-auto"></div>
                        <button type="button" className="btn btn-secondary" data-bs-dismiss="modal">Скасувати</button>
                        <button type="button" className={`btn ${isValid ? "btn-primary" : "btn-secondary-outline"}`} onClick={authenticateClick} disabled={!isValid}>Вхід</button>
                    </div>
                </div>
            </div>
        </div>
    );
}
}