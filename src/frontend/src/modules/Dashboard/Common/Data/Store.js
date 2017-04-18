import { applyMiddleware, compose, createStore, combineReducers } from 'redux';
import thunk from 'redux-thunk';
import { autoRehydrate } from 'redux-persist';
import * as actions from './Actions'

const handleGetAPI = (state, action) => {
  return Object.assign({}, state, action.response);
};

const handleNewAPI = (state, action) => {
  // Add the item or items to the store.
  var response = action.response;
  if (!(response instanceof Array)) {
    response = [response];
  }

  return Object.assign({}, state, {
    data: response.concat(state.data)
  });
};

const handlePatchAPI = (state, action) => {
  // Find the old item in the store and replace it with the updated item.
  const index = state.data.findIndex(d => d.id === action.response.id);
  return Object.assign({}, state, {
    data: [
      ...state.data.slice(0, index),
      action.response,
      ...state.data.slice(index + 1)
    ]
  });
};

const handleDeleteAPI = (state, action) => {
  // Remove the item from the store.
  return Object.assign({}, state, {
    data: state.data.filter(d => d.id !== action.response.id)
  });
}

const posts = (state={}, action) => {
  if (action.type === actions.GET_POSTS_DONE) {
    return handleGetAPI(state, action);
  }

  return state;
};

const postScrapes = (state={}, action) => {
  if (action.type === actions.GET_SCRAPED_POSTS_DONE) {
    return handleGetAPI(state, action);
  } else if (action.type === actions.SCRAPE_POSTS_DONE) {
    return handleNewAPI(state, action);
  }

  return state;
};

const pages = (state={}, action) => {
  if (action.type === actions.GET_PAGES_DONE) {
    return handleGetAPI(state, action);
  } else if (action.type === actions.NEW_PAGE_DONE) {
    return handleNewAPI(state, action);
  } else if (action.type === actions.NEW_PAGES_DONE) {
    return handleNewAPI(state, action);
  } else if (action.type === actions.EDIT_PAGE_DONE) {
    return handlePatchAPI(state, action);
  } else if (action.type === actions.DELETE_PAGE_DONE) {
    return handleDeleteAPI(state, action);
  }

  return state;
};

const pageScrapes = (state={}, action) => {
  if (action.type === actions.GET_SCRAPED_PAGES_DONE) {
    return handleGetAPI(state, action);
  } else if (action.type === actions.SCRAPE_PAGES_DONE) {
    return handleNewAPI(state, action);
  }

  return state;
};

const error = (state = null, action) => {
  if (action.type === actions.ERROR_OCCURED) {
    return action.errorMessage;
  }

  return state;
}

const reducer = combineReducers({posts, postScrapes, pages, pageScrapes, error});

export const configureStore = () => {
  const store = compose(autoRehydrate(), applyMiddleware(thunk))(createStore)(reducer);
  return store;
}
