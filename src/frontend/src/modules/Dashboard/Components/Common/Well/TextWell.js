import React, { Component } from 'react';

class TextWell extends Component {
  render() {
    return (
      <div className="well">
        <h1>{this.props.header} <small className="text-muted">{this.props.subheader}</small></h1>
      </div>
    );
  }
}

export default TextWell;
