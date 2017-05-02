import React, { Component } from 'react';

class TextWell extends Component {
  header = () => {
    const header =  <h2>{this.props.header} <small className="text-muted">{this.props.subheader}</small></h2>;
    if (this.props.onClick) {
      return <a href="#" onClick={e => { e.preventDefault(); this.props.onClick(); }}>{header}</a>
    }

    return header;
  }

  render() {
    const className = (this.props.onClick ? 'selectable-well ' : '') + 'well';

    return (
      <div className={this.props.className}>
        <div className={className}>
          {this.header()}
        </div>
      </div>
    );
  }
}

export default TextWell;
