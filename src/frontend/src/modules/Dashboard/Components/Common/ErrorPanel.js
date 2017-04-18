import React, { Component } from 'react';

class ErrorPanel extends Component {
  static get defaultProps() { return {fullWidth: true}; }
  
  render() {
    // The user can customize the width of the panel.
    var className = 'error-panel';
    if (this.props.fullWidth) { 
      className += ' container-fluid row';
    } else {
      className += ' container';
    }

    return (
      <div className={className}>
        <div className="row">
          <div className="jumbotron">
            <h1>{this.props.title || 'Something went wrong :('}</h1>
            <p>{this.props.message}</p>
            {this.props.children}
          </div>
        </div>
      </div>
    );
  }
}

export default ErrorPanel;
