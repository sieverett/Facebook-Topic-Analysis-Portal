import React, { Component } from 'react';

class TextWell extends Component {
  render() {
    return (
      <div className={this.props.className}>
        <div className="well">
          <h2>{this.props.header} <small className="text-muted">{this.props.subheader}</small></h2>
        </div>
      </div>
    );
  }
}

export default TextWell;
