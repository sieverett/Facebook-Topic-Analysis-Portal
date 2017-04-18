import React, { Component } from 'react';

class LoadingIndicator extends Component {
  render() {
    return (
      <div className="loader-container">
        <div className="loader-parent">
          <div className="loader" />
        </div>
      </div>
    );
  }
}

export default LoadingIndicator;
