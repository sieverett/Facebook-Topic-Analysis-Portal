import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import { Router, Route, Redirect, browserHistory  } from 'react-router';
import { Provider } from 'react-redux';
import { persistStore } from 'redux-persist';

import LoadingIndicator from './modules/Dashboard/Components/Common/LoadingIndicator'

import ErrorPanel from './modules/Dashboard/Components/Common/ErrorPanel';

import Posts from './modules/Dashboard/Pages/Posts';
import PostInformation from './modules/Dashboard/Pages/PostInformation';

import ScrapePosts from './modules/Dashboard/Pages/ScrapePosts';
import PostScrapeInformation from './modules/Dashboard/Pages/PostScrapeInformation';

import Pages from './modules/Dashboard/Pages/Pages';
import PageInformation from './modules/Dashboard/Pages/PageInformation';

import ScrapePages from './modules/Dashboard/Pages/ScrapePages';
import PageScrapeInformation from './modules/Dashboard/Pages/PageScrapeInformation';

import Home from './modules/Dashboard/Pages/Home';

import Page from './modules/Dashboard/Common/Page';

import './index.css';

import { configureStore } from './modules/Dashboard/Common/Data/Store';

const error404 = (
  <ErrorPanel fullWidth={false} title="Yikes!"  message="There is no such page">
    <p><a href="/" className="btn btn-primary btn-lg" role="button">Home</a></p>
  </ErrorPanel>)
;

const routes = 
  (<Route>
    <Redirect from='/' to='/dashboard'/>
    <Redirect from='/dashboard' to='/dashboard/home'/>

    <Route path="/dashboard" component={Home}/>
    <Route path="/dashboard/home" component={Home}/>

    <Route path="/dashboard/posts" component={Posts}/>
    <Route path="/dashboard/posts/scrape" component={ScrapePosts}/>
    <Route path="/dashboard/posts/scrape/:scrapeId" component={PostScrapeInformation}/>
    <Route path="/dashboard/posts/:postId" component={PostInformation}/>

    <Route path="/dashboard/pages" component={Pages}/>
    <Route path="/dashboard/pages/scrape" component={ScrapePages}/>
    <Route path="/dashboard/pages/scrape/:scrapeId" component={PageScrapeInformation}/>
    <Route path="/dashboard/pages/:pageId" component={PageInformation}/>

    <Route path="/*" component={() => error404}/>
  </Route>)
;

var store;

class Root extends Component {
  state = {rehydrated: false}

  componentWillMount() {
    persistStore(store, {whitelist: 'pages'}, () => this.setState({rehydrated: true}));
  }

  render() {
    let rootElement;
    if (!store || !this.state.rehydrated) {
      rootElement = <LoadingIndicator />;
    } else {
      rootElement = (
        <Router history={browserHistory} createElement={this.createElement}>
          {routes}
        </Router>
      );
    }

    return (
      <Provider store={store}>
        <Page>{rootElement}</Page>
      </Provider>
    );
  }
}

const render = () => ReactDOM.render(<Root roots={routes} />, document.getElementById('root'));
store = configureStore();
store.subscribe(render);
render();
